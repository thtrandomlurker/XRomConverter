using FirstLib;
using FirstLib.FirstRead;
using FirstLib.IO;
using FirstLib.FirstRead.Common;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.Bones;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.Archives.Extensions;
using MikuMikuLibrary.Parameters;
using MikuMikuLibrary.Parameters.Extensions;
using MikuMikuLibrary.Objects;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Materials;
using MikuMikuLibrary.Objects.Processing;
using System.Numerics;
using MikuMikuLibrary.Motions;
using MikuMikuLibrary.Objects.Extra.Parameters;
using MikuMikuLibrary.Objects.Extra.Blocks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MikuMikuLibrary.Extensions;
using MikuMikuLibrary.Objects.Extra;
using MikuMikuLibrary.Sprites;
using System.Drawing;
using MikuMikuLibrary.Textures.Processing;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;
using System.Reflection.PortableExecutable;
using MikuMikuLibrary.Cryptography;
using System.Runtime.InteropServices;

namespace XRomConverter;

internal enum OsageSettingPartType
{
    LEFT,
    RIGHT,
    LONG_C
}

internal class OsageSettingParameter
{
    public int EXF;
    public OsageSettingPartType Parts;
    public string Root;

    public OsageSettingParameter()
    {
        Root = "";
    }
}
internal class OsageSettingCategory
{
    public string Name;
    public List<OsageSettingParameter> Osg;

    public OsageSettingCategory()
    {
        Name = "";
        Osg = new List<OsageSettingParameter>();
    }
}

internal class OsageSettingObject
{
    public string Category;
    public string Name;

    public OsageSettingObject()
    {
        Category = "";
        Name = "";
    }
}

internal class OsageSetting
{
    public List<OsageSettingCategory> Categories;
    public List<OsageSettingObject> Objects;

    public OsageSetting()
    {
        Categories = new List<OsageSettingCategory>();
        Objects = new List<OsageSettingObject>();
    }

}

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: XRomConverter.exe \"X Rom Path\" \"MegaMix+ Path\" \"Output Path\" [OPTIONS]");
            return;
        }

        PSARC? arc = null;

        if (Path.Exists(Path.Combine(args[0], "data.psarc")))
        {
            arc = new PSARC();
            arc.Load(Path.Combine(args[0], "data.psarc"));
        }

        // tree setup

        if (!Path.Exists(args[2]))
        {
            Directory.CreateDirectory(args[2]);
        }
        if (!Path.Exists(Path.Combine(args[2], "rom")))
        {
            Directory.CreateDirectory(args[2]);
        }
        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom", "2d"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom", "2d")));
        }
        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom", "lang2"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom", "lang2")));
        }
        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom", "objset"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom", "objset")));
        }
        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom", Path.Combine("rom", "rob")))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom", "rob")));
        }
        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom", "skin_param"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom", "skin_param")));
        }

        if (!Path.Exists(Path.Combine(args[2], "rom_ps4")))
        {
            Directory.CreateDirectory(Path.Combine(args[2], "rom_ps4"));
        }

        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom_ps4", "rom"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom_ps4", "rom")));
        }

        if (!Path.Exists(Path.Combine(args[2], "rom_ps4_dlc")))
        {
            Directory.CreateDirectory(Path.Combine(args[2], "rom_ps4_dlc"));
        }

        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom_ps4_dlc", "rom"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom_ps4_dlc", "rom")));
        }

        if (!Path.Exists(Path.Combine(args[2], "rom_switch")))
        {
            Directory.CreateDirectory(Path.Combine(args[2], "rom_switch"));
        }

        if (!Path.Exists(Path.Combine(args[2], Path.Combine("rom_switch", "rom"))))
        {
            Directory.CreateDirectory(Path.Combine(args[2], Path.Combine("rom_switch", "rom")));
        }

        // write dummy files to rom_ps4/rom_ps4_dlc/rom_switch

        using (FarcArchive chritmDummy = new FarcArchive())
        {
            MemoryStream chritmStream = new MemoryStream();
            chritmStream.WriteByte(0x23);
            for (int c = 0; c < 10; c++)
            {
                chritmDummy.Add($"{((CharacterType)c).ToString().ToLower()}itm_tbl.txt", chritmStream, true);
            }

            MemoryStream farcMemStream = new MemoryStream();

            chritmDummy.Save(Path.Combine(args[2], "rom_ps4", "rom", "chritm_prop.farc"));
            File.Copy(Path.Combine(args[2], "rom_ps4", "rom", "chritm_prop.farc"), Path.Combine(args[2], "rom_ps4_dlc", "rom", "mdata_chritm_prop.farc"), true);
            File.Copy(Path.Combine(args[2], "rom_ps4", "rom", "chritm_prop.farc"), Path.Combine(args[2], "rom_switch", "rom", "chritm_prop.farc"), true);
        }

        StreamWriter modConf = File.CreateText(Path.Combine(args[2], "config.toml"));

        modConf.WriteLine("enabled = true");
        modConf.WriteLine("include = [\".\"]");

        modConf.WriteLine("name = \"Project DIVA X Rob Conversion\"");

        modConf.WriteLine($"author = \"thatrandomlurker, parameters borrowed from MFMK by skyth, This folder generated by {Environment.UserName}. Do not Redistribute.\"");

        modConf.Close();

        string romPath = args[0];

        ADCT firstRead = new ADCT();

        XReader reader = new XReader(File.Open(Path.Combine(romPath, "firstread.bin"), FileMode.Open));
        // check magic
        string magic = reader.ReadString(FirstLib.IO.StringFormat.FixedLength, 4);
        reader.Seek(0, XSeekOrigin.Begin);

        if (magic == "DIVA")
        {
            MemoryStream dec = DivafileDecryptor.DecryptToMemoryStream(reader.BaseStream);

            reader.Close();
            reader = new XReader(dec);
        }

        try
        {
            firstRead.Read(reader);
        }
        catch (InvalidDataException)
        {
            Console.WriteLine("The firstread.bin appears to be from an encrypted dump. This is NOT DIVAFILE encryption, but rather PFS encryption.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception occurred: {ex.Message}");
            return;
        }

        reader.Close();

        bool isXHD = firstRead.AddonContentContainers.Any(x => x.Paths.Any(x => x.FileName.EndsWith("emcs")));

        if (isXHD)
        {
            Console.WriteLine("XHD is currently unsupported, Please select a valid Project Diva X rom folder with unpacked data.");
            return;
        }

        // First we need to cache object set ids and object ids. X does this normally for object set ids only.

        Dictionary<uint, string> objsetHashNameMap = new Dictionary<uint, string>();
        Dictionary<uint, string> objectHashNameMap = new Dictionary<uint, string>();
        Dictionary<uint, string> textureHashNameMap = new Dictionary<uint, string>();

        objectHashNameMap.Add(MikuMikuLibrary.Hashes.MurmurHash.Calculate("NULL"), "NULL");

        foreach (var objfile in firstRead.AddonContentContainers[0].ObjsetFlist)
        {
            FarcArchive objFarc = null;
            ACCTPath objPathEntry = firstRead.AddonContentContainers[0].Paths.First(x => x.FileName == objfile);
            if (objPathEntry.Flags == ACCTPathMode.Packed)
            {
                if (arc != null)
                {
                    objFarc = BinaryFile.Load<FarcArchive>(arc.Open(objfile));
                }
                else
                {
                    objFarc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], "data", objfile));
                }
            }
            else
            {
                objFarc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], objPathEntry.FilePath, objPathEntry.FileName));
            }

            string objsetName = Path.GetFileNameWithoutExtension(objfile);

            ObjectDatabase objdb = objFarc.Open<ObjectDatabase>($"{objsetName}.osi");
            TextureDatabase texdb = objFarc.Open<TextureDatabase>($"{objsetName}.txi");

            // X only stores one OBJSET per farc.
            if (!objsetHashNameMap.ContainsKey(objdb.ObjectSets[0].Id))
                objsetHashNameMap.Add(objdb.ObjectSets[0].Id, objdb.ObjectSets[0].Name);

            foreach (var obj in objdb.ObjectSets[0].Objects)
            {
                if (!objectHashNameMap.ContainsKey(obj.Id))
                    objectHashNameMap.Add(obj.Id, obj.Name);
            }

            foreach (var tex in texdb.Textures)
            {
                if (!textureHashNameMap.ContainsKey(tex.Id))
                    textureHashNameMap.Add(tex.Id, tex.Name);
            }
        }

        CpkArchive cpk = BinaryFile.Load<CpkArchive>(Path.Combine(args[1], "diva_main.cpk"));


        BoneData mmBoneData = cpk.Open<BoneData>("rom/bone_data.bin");
        ObjectDatabase mmObjectData = cpk.Open<ObjectDatabase>("rom_steam/rom/objset/obj_db.bin");
        TextureDatabase mmTextureData = cpk.Open<TextureDatabase>("rom_steam/rom/objset/tex_db.bin");
        MotionDatabase mmMotionData = cpk.Open<FarcArchive>("rom_ps4/rom/rob/mot_db.farc").Open<MotionDatabase>("mot_db.bin");
        SpriteDatabase mmSpriteData = cpk.Open<SpriteDatabase>("rom_steam/rom/2d/spr_db.bin");

        BoneData xBoneData = null;

        ACCTPath bonePathentry = firstRead.AddonContentContainers[0].Paths.First(x => x.FileName == "bone_data.bon");
        if (bonePathentry.Flags == ACCTPathMode.Packed)
        {
            if (arc != null)
            {
                xBoneData = BinaryFile.Load<BoneData>(arc.Open("bone_data.bon"));
            }
            else
            {
                xBoneData = BinaryFile.Load<BoneData>(Path.Combine(args[0], "data", "bone_data.bon"));
            }
        }
        else
        {
            xBoneData = BinaryFile.Load<BoneData>(Path.Combine(args[0], bonePathentry.FilePath, bonePathentry.FileName));
        }

        Dictionary<int, string> charaNames = new Dictionary<int, string>() { { 0, "MIKU"},
                                                                             { 1, "RIN" },
                                                                             { 2, "LEN" },
                                                                             { 3, "LUKA"},
                                                                             { 4, "NERU"},
                                                                             { 5, "HAKU"},
                                                                             { 6, "KAITO"},
                                                                             { 7, "MEIKO"},
                                                                             { 8, "SAKINE"},
                                                                             { 9, "TETO"},
                                                                             { 10, "MIKU"}}; // Technically EXTRA, but EXTRA doesn't exist in MM.

        Dictionary<string, List<string>> motClothSkirtOsageChains = new Dictionary<string,List<string>>()
        {
            {"j_cloth_skirt_b_000_wj", new List<string>(){"j_cloth_skirt_b_000_wj"} },
            {"j_cloth_skirt_f_000_wj", new List<string>(){"j_cloth_skirt_f_000_wj"} },
            {"j_cloth_skirt_l_02_000_wj", new List<string>(){"j_cloth_skirt_l_02_000_wj"} },
            {"j_cloth_skirt_l_04_000_wj", new List<string>(){"j_cloth_skirt_l_04_000_wj"} },
            {"j_cloth_skirt_l_06_000_wj", new List<string>(){"j_cloth_skirt_l_06_000_wj"} },
            {"j_cloth_skirt_r_02_000_wj", new List<string>(){"j_cloth_skirt_r_02_000_wj"} },
            {"j_cloth_skirt_r_04_000_wj", new List<string>(){"j_cloth_skirt_r_04_000_wj"} },
            {"j_cloth_skirt_r_06_000_wj", new List<string>(){"j_cloth_skirt_r_06_000_wj"} },
        };
        
        Dictionary<string, List<string>> motClothLongOsageChains = new Dictionary<string, List<string>>()
        {
            {"j_cloth_long_f_000_wj", new List<string>(){"j_cloth_long_f_000_wj", "j_cloth_long_f_001_wj", "j_cloth_long_f_002_wj" } },
            {"j_cloth_long_b_000_wj", new List<string>(){"j_cloth_long_b_000_wj", "j_cloth_long_b_001_wj", "j_cloth_long_b_002_wj" } },
            {"j_cloth_long_l_01_000_wj", new List<string>(){"j_cloth_long_l_01_000_wj", "j_cloth_long_l_01_001_wj", "j_cloth_long_l_01_002_wj" } },
            {"j_cloth_long_l_02_000_wj", new List<string>(){"j_cloth_long_l_02_000_wj", "j_cloth_long_l_02_001_wj", "j_cloth_long_l_02_002_wj" } },
            {"j_cloth_long_l_03_000_wj", new List<string>(){"j_cloth_long_l_03_000_wj", "j_cloth_long_l_03_001_wj", "j_cloth_long_l_03_002_wj" } },
            {"j_cloth_long_r_01_000_wj", new List<string>(){"j_cloth_long_r_01_000_wj", "j_cloth_long_r_01_001_wj", "j_cloth_long_r_01_002_wj" } },
            {"j_cloth_long_r_02_000_wj", new List<string>(){"j_cloth_long_r_02_000_wj", "j_cloth_long_r_02_001_wj", "j_cloth_long_r_02_002_wj" } },
            {"j_cloth_long_r_03_000_wj", new List<string>(){"j_cloth_long_r_03_000_wj", "j_cloth_long_r_03_001_wj", "j_cloth_long_r_03_002_wj" } },
        };

        Dictionary<string, List<string>> motHairTwinOsageChains = new Dictionary<string, List<string>>()
        {
            {"j_hair_twin_l_000_wj", new List<string>(){"j_hair_twin_l_000_wj", "j_hair_twin_l_001_wj", "j_hair_twin_l_002_wj", "j_hair_twin_l_003_wj", "j_hair_twin_l_004_wj", "j_hair_twin_l_005_wj" } },
            {"j_hair_twin_r_000_wj", new List<string>(){"j_hair_twin_r_000_wj", "j_hair_twin_r_001_wj", "j_hair_twin_r_002_wj", "j_hair_twin_r_003_wj", "j_hair_twin_r_004_wj", "j_hair_twin_r_005_wj" } },
        };

        Dictionary<string, List<string>> motHairLongOsageChains = new Dictionary<string, List<string>>()
        {
            {"j_hair_long_c_000_wj", new List<string>(){"j_hair_long_c_000_wj", "j_hair_long_c_001_wj", "j_hair_long_c_002_wj", "j_hair_long_c_003_wj", "j_hair_long_c_004_wj"}},
            {"j_hair_long_l_01_000_wj", new List<string>(){"j_hair_long_l_01_000_wj", "j_hair_long_l_01_001_wj", "j_hair_long_l_01_002_wj"}},
            {"j_hair_long_r_01_000_wj", new List<string>(){"j_hair_long_r_01_000_wj", "j_hair_long_r_01_001_wj", "j_hair_long_r_01_002_wj"}},
        };

        Dictionary<string, string> osageChainParents = new Dictionary<string, string>()
        {
            // cloth_skirt
            {"j_cloth_skirt_b_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_f_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_l_02_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_l_04_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_l_06_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_r_02_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_r_04_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_skirt_r_06_000_wj", "kl_kosi_etc_wj"},
            // cloth_long
            {"j_cloth_long_b_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_b_001_wj", "j_cloth_long_b_000_wj"},
            {"j_cloth_long_b_002_wj", "j_cloth_long_b_001_wj"},
            {"j_cloth_long_f_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_f_001_wj", "j_cloth_long_f_000_wj"},
            {"j_cloth_long_f_002_wj", "j_cloth_long_f_001_wj"},
            {"j_cloth_long_l_01_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_l_01_001_wj", "j_cloth_long_l_01_000_wj"},
            {"j_cloth_long_l_01_002_wj", "j_cloth_long_l_01_001_wj"},
            {"j_cloth_long_l_02_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_l_02_001_wj", "j_cloth_long_l_02_000_wj"},
            {"j_cloth_long_l_02_002_wj", "j_cloth_long_l_02_001_wj"},
            {"j_cloth_long_l_03_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_l_03_001_wj", "j_cloth_long_l_03_000_wj"},
            {"j_cloth_long_l_03_002_wj", "j_cloth_long_l_03_001_wj"},
            {"j_cloth_long_r_01_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_r_01_001_wj", "j_cloth_long_r_01_000_wj"},
            {"j_cloth_long_r_01_002_wj", "j_cloth_long_r_01_001_wj"},
            {"j_cloth_long_r_02_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_r_02_001_wj", "j_cloth_long_r_02_000_wj"},
            {"j_cloth_long_r_02_002_wj", "j_cloth_long_r_02_001_wj"},
            {"j_cloth_long_r_03_000_wj", "kl_kosi_etc_wj"},
            {"j_cloth_long_r_03_001_wj", "j_cloth_long_r_03_000_wj"},
            {"j_cloth_long_r_03_002_wj", "j_cloth_long_r_03_001_wj"},
            // hair_twin
            {"j_hair_twin_l_000_wj", "j_kao_wj"},
            {"j_hair_twin_l_001_wj", "j_hair_twin_l_000_wj"},
            {"j_hair_twin_l_002_wj", "j_hair_twin_l_001_wj"},
            {"j_hair_twin_l_003_wj", "j_hair_twin_l_002_wj"},
            {"j_hair_twin_l_004_wj", "j_hair_twin_l_003_wj"},
            {"j_hair_twin_l_005_wj", "j_hair_twin_l_004_wj"},
            {"j_hair_twin_r_000_wj", "j_kao_wj"},
            {"j_hair_twin_r_001_wj", "j_hair_twin_r_000_wj"},
            {"j_hair_twin_r_002_wj", "j_hair_twin_r_001_wj"},
            {"j_hair_twin_r_003_wj", "j_hair_twin_r_002_wj"},
            {"j_hair_twin_r_004_wj", "j_hair_twin_r_003_wj"},
            {"j_hair_twin_r_005_wj", "j_hair_twin_r_004_wj"},
            //hair_long
            {"j_hair_long_c_000_wj", "j_kao_wj"},
            {"j_hair_long_c_001_wj", "j_hair_long_c_000_wj"},
            {"j_hair_long_c_002_wj", "j_hair_long_c_001_wj"},
            {"j_hair_long_c_003_wj", "j_hair_long_c_002_wj"},
            {"j_hair_long_c_004_wj", "j_hair_long_c_003_wj"},
            {"j_hair_long_l_01_000_wj", "j_kao_wj"},
            {"j_hair_long_l_01_001_wj", "j_hair_long_l_01_000_wj"},
            {"j_hair_long_l_01_002_wj", "j_hair_long_l_01_001_wj"},
            {"j_hair_long_r_01_000_wj", "j_kao_wj"},
            {"j_hair_long_r_01_001_wj", "j_hair_long_r_01_000_wj"},
            {"j_hair_long_r_01_002_wj", "j_hair_long_r_01_001_wj"},
        };

        // Referenced from MFMK Alpha v2.6, pairs mirrored for equality

        Dictionary<string, float> osageNodeLengths = new Dictionary<string, float>()
        {
            // cloth_skirt
            {"j_cloth_skirt_f_000_wj", 0.22828364f},
            {"j_cloth_skirt_b_000_wj", 0.3169026f},
            {"j_cloth_skirt_l_02_000_wj", 0.19258642f},
            {"j_cloth_skirt_l_04_000_wj", 0.26696336f},
            {"j_cloth_skirt_l_06_000_wj", 0.23032296f},
            {"j_cloth_skirt_r_02_000_wj", 0.19258642f},
            {"j_cloth_skirt_r_04_000_wj", 0.26696336f},
            {"j_cloth_skirt_r_06_000_wj", 0.23032296f},
            // cloth_long
            {"j_cloth_long_f_000_wj", 0.20000039f},
            {"j_cloth_long_f_001_wj", 0.22999932f},
            {"j_cloth_long_f_002_wj", 0.22999932f},
            {"j_cloth_long_b_000_wj", 0.1800004f},
            {"j_cloth_long_b_001_wj", 0.1999994f},
            {"j_cloth_long_b_002_wj", 0.1999994f},
            {"j_cloth_long_l_01_000_wj", 0.20000039f},
            {"j_cloth_long_l_01_001_wj", 0.22999938f},
            {"j_cloth_long_l_01_002_wj", 0.22999938f},
            {"j_cloth_long_l_02_000_wj", 0.20000042f},
            {"j_cloth_long_l_02_001_wj", 0.21999964f},
            {"j_cloth_long_l_02_002_wj", 0.21999964f},
            {"j_cloth_long_l_03_000_wj", 0.18000035f},
            {"j_cloth_long_l_03_001_wj", 0.19999975f},
            {"j_cloth_long_l_03_002_wj", 0.19999975f},
            {"j_cloth_long_r_01_000_wj", 0.20000039f},
            {"j_cloth_long_r_01_001_wj", 0.22999938f},
            {"j_cloth_long_r_01_002_wj", 0.22999938f},
            {"j_cloth_long_r_02_000_wj", 0.20000042f},
            {"j_cloth_long_r_02_001_wj", 0.21999964f},
            {"j_cloth_long_r_02_002_wj", 0.21999964f},
            {"j_cloth_long_r_03_000_wj", 0.18000035f},
            {"j_cloth_long_r_03_001_wj", 0.19999975f},
            {"j_cloth_long_r_03_002_wj", 0.19999975f},
            // hair_twin
            {"j_hair_twin_l_000_wj", 0.222f},
            {"j_hair_twin_l_001_wj", 0.222f},
            {"j_hair_twin_l_002_wj", 0.222f},
            {"j_hair_twin_l_003_wj", 0.222f},
            {"j_hair_twin_l_004_wj", 0.222f},
            {"j_hair_twin_l_005_wj", 0.222f},
            {"j_hair_twin_r_000_wj", 0.222f},
            {"j_hair_twin_r_001_wj", 0.222f},
            {"j_hair_twin_r_002_wj", 0.222f},
            {"j_hair_twin_r_003_wj", 0.222f},
            {"j_hair_twin_r_004_wj", 0.222f},
            {"j_hair_twin_r_005_wj", 0.222f},
            // hair_long
            {"j_hair_long_c_000_wj", 0.17000431f},
            {"j_hair_long_c_001_wj", 0.15999728f},
            {"j_hair_long_c_002_wj", 0.22000293f},
            {"j_hair_long_c_003_wj", 0.17999971f},
            {"j_hair_long_c_004_wj", 0.17999971f},
            {"j_hair_long_l_01_000_wj", 0.120002985f},
            {"j_hair_long_l_01_001_wj", 0.19999824f},
            {"j_hair_long_l_01_002_wj", 0.19999824f},
            {"j_hair_long_r_01_000_wj", 0.120002985f},
            {"j_hair_long_r_01_001_wj", 0.19999824f},
            {"j_hair_long_r_01_002_wj", 0.19999824f}
        };

        // Referenced from MFMK alpha V2.6
        Dictionary<string, OsageSkinParameter> osageSkinParameterMap = new Dictionary<string, OsageSkinParameter>()
        {
            // cloth skirt
            {"j_cloth_skirt_b_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_b_osg",
                AirResistance = 0.9f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_f_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_f_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_l_02_osg" },
                         new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_r_02_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_l_02_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_r_02_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_l_02_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_l_02_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_l_04_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_l_04_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_l_04_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_l_04_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_l_06_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_l_06_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_l_06_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_l_06_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_b_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_b_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_r_02_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_r_02_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_r_04_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_r_04_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_r_04_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_r_04_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_l_06_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_r_06_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            {"j_cloth_skirt_r_06_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_skirt_r_06_osg",
                AirResistance = 0.9f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_skirt_b_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_skirt_b_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.15625f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.15625f, -0.015625f, -0.003906f) }, Radius = 0.050781f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, 0.03125f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.035156f, -0.140625f, -0.03125f) }, Radius = 0.105469f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, 0, 0) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, 0, 0) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.03125f, -0.1875f, 0) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.058594f, -0.015625f, 0.003906f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.058594f, -0.015625f, -0.003906f) }, Radius = 0.089844f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.09375f, -0.011719f, -0.007813f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.09375f, -0.011719f, 0.007813f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.25f, 0, 0) }, Radius = 0.078125f, Type = 2 }
                },
                CollisionRadius = 0,
                CollisionType = 1,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.03f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0, Weight = 1} },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            // cloth_long
            {"j_cloth_long_b_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_b_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_l_03_osg" },
                         new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_r_03_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_l_03_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_r_03_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.9f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                }, 
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_f_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_f_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_l_01_osg" },
                         new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_r_01_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_l_01_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_r_01_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_l_01_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_l_01_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_l_02_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_l_02_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_l_02_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_l_02_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_l_03_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_l_03_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_l_03_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_l_03_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_b_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_b_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_r_01_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_r_01_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_r_02_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_r_02_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_r_02_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_r_02_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_r_03_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_r_03_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_cloth_long_r_03_000_wj", new OsageSkinParameter() {
                Name = "c_cloth_long_r_03_osg",
                AirResistance = 0.8f,
                Bocs = { new OsageBocParameter() { StNode = 0, EdNode = 0, EdRoot = "c_cloth_long_b_osg" },
                         new OsageBocParameter() { StNode = 1, EdNode = 1, EdRoot = "c_cloth_long_b_osg" }
                },
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0.0625f, 0, -0.00390625f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0.0625f, 0, 0.00390625f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.367188f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.375f, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_r_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Bone1 = { Name = "kl_asi_l_wj_co", Position = new Vector3(0, 0, 0) }, Radius = 0.09375f, Type = 2 }
                },
                CollisionRadius = 0.01f,
                CollisionType = 0,
                Force = 0.015f,
                ForceGain = 0.7f,
                Friction = 1,
                HingeY = 180f,
                HingeZ = 180f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.01f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = 0f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },

            // hair_twin
            {"j_hair_twin_l_000_wj", new OsageSkinParameter() {
                Name = "c_hair_twin_l_osg",
                AirResistance = 0.1f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "kl_mune_b_wj", Position = new Vector3(0f, 0.097656f, 0f) }, Bone1 = { Name = "n_hara_cp", Position = new Vector3(0, -0.0625f, 0) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_l_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_l_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_r_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_r_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.027344f, -0.203125f, 0.046875f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.023438f, -0.203125f, -0.046875f) }, Radius = 0.15625f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kao_wj", Position = new Vector3(0f, 0f, 0.074219f) }, Bone1 = { Name = "j_kao_wj", Position = new Vector3(0.058594f, 0f, 0.074219f) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                },
                CollisionRadius = 0f,
                CollisionType = 1,
                Force = 0.01f,
                ForceGain = 0.5f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 12f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 15f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 18f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 21f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 24f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 21f}
                },
                RotationY = -30f,
                RotationZ = -34f,
                Stiffness = 0f,
                WindAffection = 2f
            } },
            {"j_hair_twin_r_000_wj", new OsageSkinParameter() {
                Name = "c_hair_twin_r_osg",
                AirResistance = 0.1f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "kl_mune_b_wj", Position = new Vector3(0f, 0.097656f, 0f) }, Bone1 = { Name = "n_hara_cp", Position = new Vector3(0, -0.0625f, 0) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_l_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_l_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_r_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_r_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.027344f, -0.203125f, 0.046875f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.023438f, -0.203125f, -0.046875f) }, Radius = 0.15625f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kao_wj", Position = new Vector3(0f, 0f, 0.074219f) }, Bone1 = { Name = "j_kao_wj", Position = new Vector3(0.058594f, 0f, 0.074219f) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                },
                CollisionRadius = 0f,
                CollisionType = 1,
                Force = 0.01f,
                ForceGain = 0.5f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 12f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 15f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 18f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 21f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 24f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0.5f, Weight = 21f}
                },
                RotationY = 30f,
                RotationZ = -34f,
                Stiffness = 0f,
                WindAffection = 2f
            } },

            // hair_long
            {"j_hair_long_c_000_wj", new OsageSkinParameter() {
                Name = "c_hair_long_c_osg",
                AirResistance = 0.8f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "kl_mune_b_wj", Position = new Vector3(0f, 0.097656f, 0f) }, Bone1 = { Name = "n_hara_cp", Position = new Vector3(0, -0.0625f, 0) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_l_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_l_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_r_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_r_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.027344f, -0.203125f, 0.046875f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.023438f, -0.203125f, -0.046875f) }, Radius = 0.15625f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kao_wj", Position = new Vector3(0f, 0f, 0.074219f) }, Bone1 = { Name = "j_kao_wj", Position = new Vector3(0.058594f, 0f, 0.074219f) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                },
                CollisionRadius = 0f,
                CollisionType = 1,
                Force = 0.01f,
                ForceGain = 0.6f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = -5f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_hair_long_l_01_000_wj", new OsageSkinParameter() {
                Name = "c_hair_long_l_01_osg",
                AirResistance = 0.8f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "kl_mune_b_wj", Position = new Vector3(0f, 0.097656f, 0f) }, Bone1 = { Name = "n_hara_cp", Position = new Vector3(0, -0.0625f, 0) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_l_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_l_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_r_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_r_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.027344f, -0.203125f, 0.046875f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.023438f, -0.203125f, -0.046875f) }, Radius = 0.15625f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kao_wj", Position = new Vector3(0f, 0f, 0.074219f) }, Bone1 = { Name = "j_kao_wj", Position = new Vector3(0.058594f, 0f, 0.074219f) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                },
                CollisionRadius = 0f,
                CollisionType = 1,
                Force = 0.01f,
                ForceGain = 0.6f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = -5f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } },
            {"j_hair_long_r_01_000_wj", new OsageSkinParameter() {
                Name = "c_hair_long_r_01_osg",
                AirResistance = 0.8f,
                Collisions = { new OsageCollisionParameter() { Bone0 = { Name = "kl_mune_b_wj", Position = new Vector3(0f, 0.097656f, 0f) }, Bone1 = { Name = "n_hara_cp", Position = new Vector3(0, -0.0625f, 0) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_l_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0.1875f, 0f, 0f) }, Bone1 = { Name = "j_kata_r_wj_cu", Position = new Vector3(0f, 0f, 0f) }, Radius = 0.039063f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_l_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_l_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_ude_r_wj", Position = new Vector3(0.007813f, 0f, 0f) }, Bone1 = { Name = "j_ude_r_wj", Position = new Vector3(0.113281f, 0f, 0f) }, Radius = 0.042969F, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.027344f, -0.203125f, 0.046875f) }, Bone1 = { Name = "kl_kosi_etc_wj", Position = new Vector3(0.023438f, -0.203125f, -0.046875f) }, Radius = 0.15625f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_kao_wj", Position = new Vector3(0f, 0f, 0.074219f) }, Bone1 = { Name = "j_kao_wj", Position = new Vector3(0.058594f, 0f, 0.074219f) }, Radius = 0.113281f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_l_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_momo_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_momo_r_wj", Position = new Vector3(0.347656f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_l_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_l_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                               new OsageCollisionParameter() { Bone0 = { Name = "j_sune_r_wj", Position = new Vector3(0f, 0f, 0f) }, Bone1 = { Name = "j_sune_r_wj", Position = new Vector3(0.457031f, 0f, 0f) }, Radius = 0.085938f, Type = 2 },
                },
                CollisionRadius = 0f,
                CollisionType = 1,
                Force = 0.01f,
                ForceGain = 0.6f,
                Friction = 1,
                HingeY = 90f,
                HingeZ = 90f,
                InitRotationY = 0f,
                InitRotationZ = 0f,
                MoveCancel = 0f,
                Nodes = { new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f},
                          new OsageNodeParameter() { Radius = 0.04f, HingeYMax = 180f, HingeYMin = -180f, HingeZMax = 180f, HingeZMin = -180f, InertialCancel = 0f, Weight = 3f}
                },
                RotationY = 0f,
                RotationZ = -5f,
                Stiffness = 0f,
                WindAffection = 0.5f
            } }
        };
        Dictionary<string, List<string>> osageExpHelpers = new Dictionary<string, List<string>>()
        {
            // cloth_skirt
            {"j_cloth_skirt_b_000_wj", new List<string>(){""}},
            {"j_cloth_skirt_f_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex v 3.n_momo_a_r_wj_cd_ex g + n 0.5 g * n 0.75 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex v 5.n_momo_a_r_wj_cd_ex g + n 0.5 g * g min"}},
            {"j_cloth_skirt_l_02_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex n 0.5 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex g min"}},
            {"j_cloth_skirt_l_04_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex n 0.5 g * g min"}},
            {"j_cloth_skirt_l_06_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex n 0.5 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex n 0.25 g * g min"}},
            {"j_cloth_skirt_r_02_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex n 0.5 g * g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex g min"}},
            {"j_cloth_skirt_r_04_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex n 0.5 g * g min"}},
            {"j_cloth_skirt_r_06_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex n 0.5 g * g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex n 0.25 g * g min"}},
            // cloth_long
            {"j_cloth_long_b_000_wj", new List<string>(){""}},
            {"j_cloth_long_f_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex v 3.n_momo_a_r_wj_cd_ex g + n 0.5 g * n 0.75 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex v 5.n_momo_a_r_wj_cd_ex g + n 0.5 g * g min"}},
            {"j_cloth_long_l_01_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex n 0.5 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex g min"}},
            {"j_cloth_long_l_02_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex n 0.5 g * g min"}},
            {"j_cloth_long_l_03_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_l_wj_cd_ex n 0.5 g * g min", "= 5 n 0 v 5.n_momo_a_l_wj_cd_ex n 0.25 g * g min"}},
            {"j_cloth_long_r_01_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex n 0.5 g * g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex g min"}},
            {"j_cloth_long_r_02_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex n 0.5 g * g min"}},
            {"j_cloth_long_r_03_000_wj", new List<string>(){"= 3 n 0 v 3.n_momo_a_r_wj_cd_ex n 0.5 g * g max", "= 5 n 0 v 5.n_momo_a_r_wj_cd_ex n 0.25 g * g min"}},
            // hair_twin
            {"j_hair_twin_l_000_wj", new List<string>(){"= 3 v 5.kl_kubi f neg", "= 4 v 3.cl_kao", "= 5 n 90 v 5.j_kao_wj g -"}},
            {"j_hair_twin_r_000_wj", new List<string>(){"= 3 v 5.kl_kubi f neg", "= 4 v 3.cl_kao", "= 5 n 90 v 5.j_kao_wj g -"}},
            // hair_long
            {"j_hair_long_c_000_wj", new List<string>(){"= 3 v 5.kl_kubi f neg", "= 4 v 3.cl_kao", "= 5 n 90 v 5.j_kao_wj g -"}},
            {"j_hair_long_l_01_000_wj", new List<string>(){"= 3 v 5.kl_kubi f neg", "= 4 v 3.cl_kao", "= 5 n 90 v 5.j_kao_wj g -"}},
            {"j_hair_long_r_01_000_wj", new List<string>(){"= 3 v 5.kl_kubi f neg", "= 4 v 3.cl_kao", "= 5 n 90 v 5.j_kao_wj g -"}},
        };

        ModuleTable moduleTable = firstRead.AddonContentContainers[0].ParameterDatabases[0].ModuleTable;

        StreamWriter langToml = File.CreateText(Path.Combine(args[2], "rom", "lang2", "mod_str_array.toml"));
        SpriteDatabase modSprDb = new SpriteDatabase();

        ParameterTreeWriter gmModuleId = new ParameterTreeWriter();

        int curModule = 0;

        gmModuleId.PushScope("module");

        uint spriteSetIdBase = 10010000;
        uint spriteIdBase = 10011000;
        uint spriteTexIdBase = 10012000;

        foreach (var module in moduleTable.Modules)
        {
            // try sprite conversion

            if (!args.Contains("-i"))
            {

                if (firstRead.AddonContentContainers[0].Auth2DFlist.Contains($"spr_mdl_thmb{module.ModuleID:D3}l.farc"))
                {
                    ACCTPath sprPathEntry = firstRead.AddonContentContainers[0].Paths.First(x => x.FileName == $"spr_mdl_thmb{module.ModuleID:D3}l.farc");
                    string sprPath = null;
                    if (sprPathEntry.Flags == ACCTPathMode.Packed)
                    {
                        sprPath = Path.Combine(args[0], "data", $"spr_mdl_thmb{module.ModuleID:D3}l.farc");
                    }
                    else
                    {
                        sprPath = Path.Combine(args[0], sprPathEntry.FilePath, sprPathEntry.FileName);
                    }
                    FarcArchive xSprFarc;
                    if (arc != null)
                    {
                        xSprFarc = BinaryFile.Load<FarcArchive>(arc.Open(sprPathEntry.FileName));
                    }
                    else
                    {
                        xSprFarc = BinaryFile.Load<FarcArchive>(sprPath);
                    }
                    SpriteDatabase spi = xSprFarc.Open<SpriteDatabase>($"spr_mdl_thmb{module.ModuleID:D3}l.spi");
                    SpriteSet spr = xSprFarc.Open<SpriteSet>($"spr_mdl_thmb{module.ModuleID:D3}l.spr");

                    if (mmSpriteData.SpriteSets.FirstOrDefault(x => x.Name == $"SPR_SEL_MD{module.ModuleID:D3}CMN") == null)
                    {
                        SpriteSetInfo modSprSetInfo = new SpriteSetInfo()
                        {
                            Id = spriteSetIdBase,
                            Name = $"SPR_SEL_MD{module.ModuleID:D3}CMN",
                            FileName = $"spr_sel_md{module.ModuleID:D3}cmn.bin",
                            Sprites = { new SpriteInfo() { Name = $"SPR_SEL_MD{module.ModuleID:D3}CMN_MD_IMG", Id = spriteIdBase, Index = 0 } },
                            Textures = { new SpriteTextureInfo() { Name = $"SPR_SEL_MD{module.ModuleID:D3}CMN_MERGE_BC5COMP", Id = spriteTexIdBase, Index = 0 } }
                        };
                        spriteSetIdBase += 1;
                        spriteIdBase += 1;
                        spriteTexIdBase += 1;
                        modSprDb.SpriteSets.Add(modSprSetInfo);
                    }

                    using (Bitmap cropped = SpriteCropper.Crop(spr.Sprites[0], spr))
                    {
                        using (Bitmap scaled = new Bitmap(512, 512))
                        {
                            using (Graphics gfx = Graphics.FromImage(scaled))
                            {
                                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                gfx.DrawImage(cropped, 2, 2, 408, 408);
                            }

                            scaled.RotateFlip(RotateFlipType.RotateNoneFlipY);

                            spr.TextureSet.Textures[0] = TextureEncoder.EncodeYCbCrFromBitmap(scaled);
                            spr.Sprites[0].Name = "MD_IMG";
                            spr.Sprites[0].ResolutionMode = ResolutionMode.HDTV1080;
                            spr.Sprites[0].X = 2;
                            spr.Sprites[0].Y = 2;
                            spr.Sprites[0].Width = 408;
                            spr.Sprites[0].Height = 494;
                            spr.Sprites[0].RectangleBegin = new Vector2(spr.Sprites[0].X / 512, spr.Sprites[0].Y / 512);
                            spr.Sprites[0].RectangleEnd = new Vector2(spr.Sprites[0].X / 512 + spr.Sprites[0].Width / 512, spr.Sprites[0].Y / 512 + spr.Sprites[0].Height / 512);

                            using (FarcArchive modSprFarc = new FarcArchive())
                            {
                                MemoryStream sprStream = new MemoryStream();
                                spr.Format = BinaryFormat.DT;
                                spr.Save(sprStream, true);

                                modSprFarc.Add($"spr_sel_md{module.ModuleID:D3}cmn.bin", sprStream, false);
                                modSprFarc.Save(Path.Combine(args[2], "rom", "2d", $"spr_sel_md{module.ModuleID:D3}cmn.farc"));
                            }
                        }
                    }
                }
            }

            string moduleName = "";

            foreach (var stra in firstRead.AddonContentContainers[0].StringArrays)
            {
                moduleName = stra.GetStringById(10010000 + module.ModuleID);
                if (moduleName != "")
                {
                    break;
                }
            }

            langToml.WriteLine($"module.{module.ModuleID} = \"{moduleName}\"");

            Console.WriteLine($"[GAME_MODULE_TABLE_CONVERSION] --> {moduleName}");
            gmModuleId.PushScope(curModule);
            gmModuleId.Write<int>("attr", 2);
            gmModuleId.Write<string>("chara", charaNames[((int)module.Character)]);
            gmModuleId.Write<string>("cos", $"COS_{module.CosID + 1}");
            gmModuleId.Write<int>("id", module.ModuleID);
            gmModuleId.Write<string>("name", moduleName);

            gmModuleId.Write<int>("ng", 0);
            gmModuleId.Write<int>("shop_ed_day", 1);
            gmModuleId.Write<int>("shop_ed_month", 1);
            gmModuleId.Write<int>("shop_ed_year", 2029);
            gmModuleId.Write<int>("shop_price", 0);
            gmModuleId.Write<int>("shop_st_day", 9);
            gmModuleId.Write<int>("shop_st_month", 3);
            gmModuleId.Write<int>("shop_st_year", 2025);
            gmModuleId.Write<int>("sort_index", module.SortIndex);
            gmModuleId.PopScope();

            curModule++;
        }

        modSprDb.Save(Path.Combine(args[2], "rom", "2d", "mod_spr_db.bin"));

        gmModuleId.Write("data_list.length", moduleTable.Modules.Count);

        using (FarcArchive gmModuleTbl = new FarcArchive())
        {
            MemoryStream gmModuleIdStream = new MemoryStream();
            gmModuleId.Flush(gmModuleIdStream);

            gmModuleTbl.Add("gm_module_id.bin", gmModuleIdStream, false);
            gmModuleTbl.Save(Path.Combine(args[2], "rom", "gm_module_tbl.farc"));
        }

        // gm_customize_item_tbl

        CustomizeItemTable customizeItemTable = firstRead.AddonContentContainers[0].ParameterDatabases[0].CustomizeItemTable;

        ParameterTreeWriter gmCustomizeItemId = new ParameterTreeWriter();

        int curCustomizeItem = 0;

        gmCustomizeItemId.PushScope("cstm_item");

        foreach (var custItem in customizeItemTable.CustomizeItems)
        {
            gmCustomizeItemId.PushScope(curCustomizeItem);

            gmCustomizeItemId.Write("bind_module", -1);
            gmCustomizeItemId.Write("chara", "ALL");
            gmCustomizeItemId.Write("id", custItem.ItemID);

            string custItemName = "";

            foreach (var stra in firstRead.AddonContentContainers[0].StringArrays)
            {
                custItemName = stra.GetStringById(10030000 + custItem.ItemID);
                if (custItemName != "")
                {
                    break;
                }
            }

            langToml.WriteLine($"customize.{custItem.ItemID} = \"{custItemName}\"");

            gmCustomizeItemId.Write("name", custItemName);

            gmCustomizeItemId.Write("ng", 0);
            gmCustomizeItemId.Write("obj_id", custItem.ItemNumber);
            gmCustomizeItemId.Write("parts", custItem.Part.ToString());
            gmCustomizeItemId.Write("sell_type", 1);
            gmCustomizeItemId.Write("shop_ed_day", 1);
            gmCustomizeItemId.Write("shop_ed_month", 1);
            gmCustomizeItemId.Write("shop_ed_year", 2029);
            gmCustomizeItemId.Write("shop_price", 200);
            gmCustomizeItemId.Write("shop_st_day", 9);
            gmCustomizeItemId.Write("shop_st_month", 3);
            gmCustomizeItemId.Write("shop_st_year", 2025);
            gmCustomizeItemId.Write("sort_index", custItem.U04);
            gmCustomizeItemId.PopScope();
            curCustomizeItem++;
        }
        gmCustomizeItemId.PopScope();
        gmCustomizeItemId.Write("cust_item.data_list.length", customizeItemTable.CustomizeItems.Count);
        gmCustomizeItemId.Write("patch", 0);
        gmCustomizeItemId.Write("version", 0);

        using (FarcArchive gmCustomizeItemTbl = new FarcArchive())
        {
            MemoryStream gmCustomizeItemIdStream = new MemoryStream();
            gmCustomizeItemId.Flush(gmCustomizeItemIdStream);

            gmCustomizeItemTbl.Add("gm_customize_item_id.bin", gmCustomizeItemIdStream, false);
            gmCustomizeItemTbl.Save(Path.Combine(args[2], "rom", "mod_gm_customize_item_tbl.farc"));
        }


        // Chritm_Prop

        // Also gather object sets we actually need to convert.

        List<string> conversionObjectSets = new List<string>();


        using (FarcArchive chritmProp = new FarcArchive())
        {
            for (int i = 0; i < 10; i++)
            {
                CharacterItemTable chritm = firstRead.AddonContentContainers[0].RobDatabases[0].CharacterItemTables[i];

                ParameterTreeWriter chritmTbl = new ParameterTreeWriter();

                conversionObjectSets.Add($"{((CharacterType)i).ToString().ToUpper()}ITM000");  // add the character's "base" objects. in all modern games, it's just their face.


                chritmTbl.Write("cos", chritm.Costumes, x =>
                {
                    Console.WriteLine($"[{((CharacterType)i).ToString().ToUpper()}ITM_TABLE_CONVERSION] --> COS_{x.CostumeID+1:D3}");
                    chritmTbl.Write("id", x.CostumeID);
                    chritmTbl.PushScope("item");
                    int curItmId = 0;
                    foreach (var itm in x.Parts)
                    {
                        if (itm.ItemNumber != 0)
                        {
                            chritmTbl.Write($"{curItmId}", itm.ItemNumber);
                            curItmId++;
                        }
                    }
                    chritmTbl.Write("length", curItmId);
                    chritmTbl.PopScope();
                });


                int currentItemId = 0;
                chritmTbl.PushScope("item");

                foreach (var item in chritm.Items)
                {
                    chritmTbl.PushScope(currentItemId);
                    chritmTbl.Write("attr", item.Attribute);

                    if (item.Objects.Count > 0)
                    {
                        chritmTbl.Write("data.obj", item.Objects, x =>
                        {
                            chritmTbl.Write("rpk", (sbyte)x.RPK);
                            Console.WriteLine($"[{((CharacterType)i).ToString().ToUpper()}ITM_TABLE_CONVERSION] --> Item {item.ItemNumber:D3}");
                            if (objectHashNameMap.TryGetValue(x.ObjectID, out string? value))
                                if (!value.Contains("ITM000"))
                                    chritmTbl.Write("uid", $"{value}_X");
                                else
                                    chritmTbl.Write("uid", $"{value}");
                            else
                                chritmTbl.Write("uid", $"MISSING_OBJ_{x.ObjectID}");
                        });
                    }

                    if (item.TextureChanges.Count > 0)
                    {
                        chritmTbl.Write("data.tex", item.TextureChanges, x =>
                        {
                            if (textureHashNameMap.TryGetValue(x.ReplacementTextureID, out string? replaceName))
                                chritmTbl.Write("chg", replaceName);
                            else
                                chritmTbl.Write("chg", $"MISSING_TEX_{x.ReplacementTextureID}");
                            if (textureHashNameMap.TryGetValue(x.OriginalTextureID, out string? orgName))
                                chritmTbl.Write("org", orgName);
                            else
                                chritmTbl.Write("org", $"MISSING_TEX_{x.OriginalTextureID}");
                        });
                    }
                    chritmTbl.Write("des_id", item.DestID);
                    chritmTbl.Write("exclusion", 0);
                    chritmTbl.Write("face_depth", 0);
                    chritmTbl.Write("flag", 0);

                    string res = "";
                    foreach (var stra in firstRead.AddonContentContainers[0].StringArrays)
                    {
                        res = stra.GetStringById(int.Parse($"190{i:D2}{item.ItemNumber:D4}"));
                        if (res != "")
                        {
                            break;
                        }
                    }

                    if (res == "")
                    {
                        res = $"{(CharacterType)i}ITM{item.ItemNumber:D3}";
                    }

                    chritmTbl.Write("name", res);
                    chritmTbl.Write("no", item.ItemNumber);

                    chritmTbl.PushScope("objset");

                    int curObjsetId = 0;

                    foreach (var objsetId in item.ObjectSets)
                    {
                        if (objsetHashNameMap.TryGetValue(objsetId, out string? value))
                        {
                            if (!conversionObjectSets.Contains(value))
                                conversionObjectSets.Add(value);
                            if (!value.Contains("ITM000"))
                                chritmTbl.Write($"{curObjsetId}", $"{value}_X");
                            else
                                chritmTbl.Write($"{curObjsetId}", $"{value}");
                        }
                        else
                            chritmTbl.Write($"{curObjsetId}", $"MISSING_OBJSET_{objsetId}");
                        curObjsetId++;
                    }

                    chritmTbl.Write("length", item.ObjectSets.Count);
                    chritmTbl.PopScope();

                    chritmTbl.Write("org_itm", 0);

                    chritmTbl.Write("point", 0);
                    chritmTbl.Write("sub_id", (int)item.SubID);
                    chritmTbl.Write("type", item.Type);
                    chritmTbl.PopScope();

                    currentItemId++;
                }

                foreach (var item in firstRead.AddonContentContainers[0].RobDatabases[0].CustomizeItems)
                {
                    chritmTbl.PushScope(currentItemId);
                    chritmTbl.Write("attr", item.Attribute);

                    if (item.Objects.Count > 0)
                    {
                        chritmTbl.Write("data.obj", item.Objects, x =>
                        {
                            chritmTbl.Write("rpk", (sbyte)x.RPK);
                            Console.WriteLine($"[{((CharacterType)i).ToString().ToUpper()}ITM_TABLE_CONVERSION] --> Customize Item {item.ItemNumber:D4}");
                            if (objectHashNameMap.TryGetValue(x.ObjectID, out string? value))
                                if (!value.Contains("ITM000"))
                                    chritmTbl.Write("uid", $"{value}_X");
                                else
                                    chritmTbl.Write("uid", $"{value}");
                            else
                                chritmTbl.Write("uid", $"MISSING_OBJ_{x.ObjectID}");
                        });
                    }

                    if (item.TextureChanges.Count > 0)
                    {
                        chritmTbl.Write("data.tex", item.TextureChanges, x =>
                        {
                            if (textureHashNameMap.TryGetValue(x.ReplacementTextureID, out string? replaceName))
                                chritmTbl.Write("chg", replaceName);
                            else
                                chritmTbl.Write("chg", $"MISSING_TEX_{x.ReplacementTextureID}");
                            if (textureHashNameMap.TryGetValue(x.OriginalTextureID, out string? orgName))
                                chritmTbl.Write("org", orgName);
                            else
                                chritmTbl.Write("org", $"MISSING_TEX_{x.OriginalTextureID}");
                        });
                    }
                    chritmTbl.Write("des_id", item.DestID);
                    chritmTbl.Write("exclusion", 0);
                    chritmTbl.Write("face_depth", 0);
                    chritmTbl.Write("flag", 0);

                    string res = "";
                    foreach (var stra in firstRead.AddonContentContainers[0].StringArrays)
                    {
                        res = stra.GetStringById(int.Parse($"19099{item.ItemNumber:D4}"));
                        if (res != "")
                        {
                            break;
                        }
                    }

                    if (res == "")
                    {
                        res = $"CMNITM{item.ItemNumber:D4}";
                    }

                    chritmTbl.Write("name", res);
                    chritmTbl.Write("no", item.ItemNumber);

                    chritmTbl.PushScope("objset");

                    int curObjsetId = 0;

                    foreach (var objsetId in item.ObjectSets)
                    {
                        if (objsetHashNameMap.TryGetValue(objsetId, out string? value))
                        {
                            if (!conversionObjectSets.Contains(value))
                                conversionObjectSets.Add(value);
                            if (!value.Contains("ITM000"))
                                chritmTbl.Write($"{curObjsetId}", $"{value}_X");
                            else
                                chritmTbl.Write($"{curObjsetId}", $"{value}");
                        }
                        else
                            chritmTbl.Write($"{curObjsetId}", $"MISSING_OBJSET_{objsetId}");
                    }

                    chritmTbl.Write("length", item.ObjectSets.Count);
                    chritmTbl.PopScope();

                    chritmTbl.Write("org_itm", 0);

                    chritmTbl.Write("point", 0);
                    chritmTbl.Write("sub_id", (int)item.SubID);
                    chritmTbl.Write("type", item.Type);
                    chritmTbl.PopScope();

                    currentItemId++;
                }

                chritmTbl.Write("length", chritm.Items.Count + firstRead.AddonContentContainers[0].RobDatabases[0].CustomizeItems.Count);
                chritmTbl.PopScope();


                MemoryStream chritmStream = new MemoryStream();

                chritmTbl.Flush(chritmStream);

             
                chritmProp.Add($"{((CharacterType)i).ToString().ToLower()}itm_tbl.txt", chritmStream, false);
            }

            chritmProp.Save(Path.Combine(args[2], "rom", "mod_chritm_prop.farc"));
        }



        // patch the bone data

        foreach (var xSkel in xBoneData.Skeletons)
        {
            Console.WriteLine($"[BONEDATA_CONVERSION] !!! {xSkel.Name}");
        }

        foreach (var mmSkel in mmBoneData.Skeletons)
        {

            Skeleton xSkel = xBoneData.Skeletons.First(x => x.Name == mmSkel.Name);
            Console.WriteLine($"[BONEDATA_CONVERSION] --> Converting skeleton {mmSkel.Name}");

            foreach (var mmBoneInfo in mmSkel.Bones)
            {
                int mmBonePosOfs = 0;
                int xBonePosOfs = 0;

                foreach (var bone in mmSkel.Bones)
                {
                    if (bone == mmBoneInfo)
                    {
                        break;
                    }

                    mmBonePosOfs += 1;
                    if (bone.Type == BoneType.HeadIKRotation)
                    {
                        mmBonePosOfs += 1;
                    }
                    else if (bone.Type >= BoneType.ArmIKRotation)
                    {
                        mmBonePosOfs += 2;
                    }
                }

                Bone xBoneInfo = xSkel.GetBone(mmBoneInfo.Name);

                if (xBoneInfo != null)
                {

                    foreach (var bone in xSkel.Bones)
                    {
                        if (bone == xBoneInfo)
                        {
                            break;
                        }

                        xBonePosOfs += 1;
                        if (bone.Type == BoneType.HeadIKRotation)
                        {
                            xBonePosOfs += 1;
                        }
                        else if (bone.Type >= BoneType.ArmIKRotation)
                        {
                            xBonePosOfs += 2;
                        }
                    }


                    mmSkel.Positions[mmBonePosOfs] = xSkel.Positions[xBonePosOfs];
                    if (mmBoneInfo.Type == BoneType.HeadIKRotation)
                        mmSkel.Positions[mmBonePosOfs + 1] = xSkel.Positions[xBonePosOfs + 1];

                    if (mmBoneInfo.Type >= BoneType.ArmIKRotation)
                        mmSkel.Positions[mmBonePosOfs + 2] = xSkel.Positions[xBonePosOfs + 2];
                }
            }

            mmSkel.HeelHeight = xSkel.HeelHeight;
        }

        mmBoneData.Save(Path.Combine(args[2], "rom", "bone_data.bin"));



        uint objsetBaseId = 40000;
        uint textureBaseId = 90000;

        ObjectDatabase modObjDb = new ObjectDatabase();
        TextureDatabase modTexDb = new TextureDatabase();

        if (!args.Contains("-n"))
        {
            OsageSetting osageSetting = new OsageSetting()
            { Categories = { new OsageSettingCategory() {Name = "long_tail_f", Osg = { new OsageSettingParameter() {EXF = 8, Parts = OsageSettingPartType.LEFT, Root = "c_hair_twin_l_osg"},
                                                                                       new OsageSettingParameter() {EXF = 8, Parts = OsageSettingPartType.RIGHT, Root = "c_hair_twin_r_osg"}
                                                                                     }
                                                        },
                             new OsageSettingCategory() {Name = "long_hair_f", Osg = { new OsageSettingParameter() {EXF = 0, Parts = OsageSettingPartType.LONG_C, Root = "c_hair_long_c_osg"},
                                                                                       new OsageSettingParameter() {EXF = 0, Parts = OsageSettingPartType.LONG_C, Root = "c_hair_long_l_01_osg"},
                                                                                       new OsageSettingParameter() {EXF = 0, Parts = OsageSettingPartType.LONG_C, Root = "c_hair_long_r_01_osg"}
                                                                                     }
                                                        }
                           }
            };


            foreach (var objsetName in conversionObjectSets)
            {
                Console.WriteLine($"[OBJECT_CONVERSION] --> {objsetName}");

                FarcArchive conversionFarc = null;
                ACCTPath convPathEntry = firstRead.AddonContentContainers[0].Paths.FirstOrDefault(x => x.FileName == $"{objsetName.ToLower()}.farc");
                if (convPathEntry.Flags == ACCTPathMode.Packed)
                {
                    if (arc != null)
                    {
                        conversionFarc = BinaryFile.Load<FarcArchive>(arc.Open($"{objsetName.ToLower()}.farc"));
                    }
                    else
                    {
                        conversionFarc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], "data", $"{objsetName.ToLower()}.farc"));
                    }
                }
                else
                {
                    conversionFarc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], convPathEntry.FilePath, convPathEntry.FileName));
                }

                ObjectSet xObjectSet = conversionFarc.Open<ObjectSet>($"{objsetName.ToLower()}.osd");
                TextureSet xTextureSet = conversionFarc.Open<TextureSet>($"{objsetName.ToLower()}.txd");
                TextureDatabase xTextureDatabase = conversionFarc.Open<TextureDatabase>($"{objsetName.ToLower()}.txi");

                for (int i = 0; i < xObjectSet.TextureIds.Count; i++)
                {
                    TextureInfo texInfo = xTextureDatabase.GetTextureInfo(xObjectSet.TextureIds[i]);

                    if (!modTexDb.Textures.Any(x => x.Name == texInfo.Name))
                    {
                        modTexDb.Textures.Add(new TextureInfo() { Name = texInfo.Name, Id = textureBaseId });
                        xObjectSet.TextureIds[i] = textureBaseId;
                        xTextureSet.Textures[i].Id = textureBaseId;
                    }
                    else
                    {
                        uint texid = modTexDb.GetTextureInfo(xTextureDatabase.GetTextureInfo(xObjectSet.TextureIds[i]).Name).Id;
                        xObjectSet.TextureIds[i] = texid;
                        xTextureSet.Textures[i].Id = texid;
                    }

                    textureBaseId++;
                }

                // If it's a head, move head 07 to head 08 and copy head 01 to head 07.

                if (objsetName.EndsWith("ITM000"))
                {
                    MikuMikuLibrary.Objects.Object head01 = xObjectSet.Objects.First(x => x.Name == $"{objsetName[0..3]}ITM000_ATAM_HEAD_01__DIVSKN");
                    MikuMikuLibrary.Objects.Object head07 = new MikuMikuLibrary.Objects.Object();
                    
                    foreach (var mesh in head01.Meshes)
                    {
                        head07.Meshes.Add(mesh);
                    }
                    foreach (var material in head01.Materials)
                    {
                        head07.Materials.Add(material);
                    }
                    head07.Skin = head01.Skin;
                    head07.BoundingSphere = head01.BoundingSphere;

                    head07.Name = $"{objsetName[0..3]}ITM000_ATAM_HEAD_07__DIVSKN";
                    xObjectSet.Objects.First(x => x.Name == $"{objsetName[0..3]}ITM000_ATAM_HEAD_07__DIVSKN").Name = $"{objsetName[0..3]}ITM000_ATAM_HEAD_08__DIVSKN";
                    xObjectSet.Objects.Add(head07);
                }

                ObjectSetInfo setInfo = new ObjectSetInfo() { Name = objsetName };
                if (objsetName.EndsWith("ITM000"))
                {
                    ObjectSetInfo objinfo = mmObjectData.GetObjectSetInfo(objsetName);
                    setInfo.Id = objinfo.Id == 0 ? objsetBaseId : objinfo.Id;
                }
                else
                {
                    setInfo.Name = $"{objsetName}_X";
                    setInfo.Id = objsetBaseId;
                }

                if (objsetName.EndsWith("ITM000"))
                {

                    setInfo.ArchiveFileName = $"{objsetName.ToLower()}.farc";
                    setInfo.FileName = $"{objsetName.ToLower()}_obj.bin";
                    setInfo.TextureFileName = $"{objsetName.ToLower()}_tex.bin";
                }
                else
                {

                    setInfo.ArchiveFileName = $"{objsetName.ToLower()}_x.farc";
                    setInfo.FileName = $"{objsetName.ToLower()}_x_obj.bin";
                    setInfo.TextureFileName = $"{objsetName.ToLower()}_x_tex.bin";
                }

                uint baseObjectId = 0;

                foreach (var obj in xObjectSet.Objects)
                {
                    obj.Name = obj.Name.ToLower();
                    OsageSkinParameterSet osp = new OsageSkinParameterSet();
                    ACCTPath? skpPathEntry = firstRead.AddonContentContainers[0].Paths.FirstOrDefault(x => x.FileName == $"ext_skp_{obj.Name}.osp");
                    string skpPath = null;
                    if (skpPathEntry != null)
                    {
                        Console.WriteLine("[SKIN PARAMETER] - SkinParam exists");
                        if (convPathEntry.Flags == ACCTPathMode.Packed)
                            skpPath = Path.Combine(args[0], "data", $"ext_skp_{obj.Name}.osp");
                        else
                        {
                            skpPath = Path.Combine(args[0], skpPathEntry.FilePath, skpPathEntry.FileName);
                        }
                    }
                    if (skpPath != null)
                    {
                        if (arc != null)
                        {
                            osp.Load(arc.Open(skpPathEntry.FileName));
                        }
                        else
                        {
                            osp.Load(skpPath);
                        }
                        osp.Format = BinaryFormat.DT;
                    }

                    if (objsetName.EndsWith("ITM000"))
                    {
                        ObjectInfo objinfo = mmObjectData.GetObjectInfo(obj.Name);
                        obj.Id = objinfo.Id == 0 ? baseObjectId : objinfo.Id;
                    }
                    else
                    {
                        obj.Name = $"{obj.Name}_x";
                        obj.Id = baseObjectId;
                    }

                    foreach (var mesh in obj.Meshes)
                    {
                        foreach (var submesh in mesh.SubMeshes)
                        {
                            if (submesh.PrimitiveType != PrimitiveType.TriangleStrip)
                            {
                                submesh.PrimitiveType = PrimitiveType.TriangleStrip;
                                submesh.Indices = Stripifier.Stripify(submesh.Indices);
                            }
                        }
                    }

                    setInfo.Objects.Add(new ObjectInfo() { Name = obj.Name.ToUpper(), Id = obj.Id });

                    baseObjectId++;

                    for (int m = 0; m < obj.Materials.Count; m++)
                    {
                        Material mat = obj.Materials[m];
                        Material xconvMat = new Material();
                        xconvMat.Name = mat.Name;
                        xconvMat.ShaderFlags = mat.ShaderFlags;
                        xconvMat.BlendFlags = mat.BlendFlags;
                        xconvMat.Flags = mat.Flags;
                        xconvMat.Diffuse = mat.Diffuse;
                        xconvMat.Specular = mat.Specular;
                        xconvMat.Ambient = mat.Ambient;
                        xconvMat.Emission = mat.Emission;
                        xconvMat.Shininess = mat.Shininess;
                        if (mat.ShaderName == "SKIN" || mat.ShaderName == "HAIR")
                        {
                            if (mat.MaterialTextures[0].TextureId != 212676728 && mat.MaterialTextures[0].TextureId != 1509989155)
                            {
                                xconvMat.MaterialTextures[0] = mat.MaterialTextures[0];
                                try
                                {
                                    xconvMat.MaterialTextures[0].TextureId = modTexDb.GetTextureInfo(xTextureDatabase.GetTextureInfo(xconvMat.MaterialTextures[0].TextureId).Name).Id;
                                }
                                catch
                                {
                                }
                            }
                            if (mat.MaterialTextures[1].TextureId != 212676728 && mat.MaterialTextures[1].TextureId != 1509989155)
                            {
                                xconvMat.MaterialTextures[1] = mat.MaterialTextures[4];
                                try
                                {
                                    xconvMat.MaterialTextures[1].TextureId = modTexDb.GetTextureInfo(xTextureDatabase.GetTextureInfo(xconvMat.MaterialTextures[1].TextureId).Name).Id;
                                }
                                catch
                                {
                                }
                            }
                            if (mat.MaterialTextures[3].TextureId != 212676728 && mat.MaterialTextures[3].TextureId != 1509989155)
                            {
                                xconvMat.MaterialTextures[3] = mat.MaterialTextures[3];
                                try
                                {
                                    xconvMat.MaterialTextures[3].TextureId = modTexDb.GetTextureInfo(xTextureDatabase.GetTextureInfo(xconvMat.MaterialTextures[3].TextureId).Name).Id;
                                }
                                catch
                                {
                                }
                            }
                            xconvMat.ShaderName = "ITEM";

                            ushort xMaterialFlags = 32768;
                            if (!args.Contains("-s"))
                            {
                                xMaterialFlags += 8192;
                            }
                            if (mat.ShaderName == "HAIR")
                            {
                                xMaterialFlags += 16384;
                            }
                            xconvMat.Ambient = new System.Numerics.Vector4(xconvMat.Ambient.X, xconvMat.Ambient.Y, xconvMat.Ambient.Z, (float)xMaterialFlags);
                        }
                        else
                        {
                            for (int mt = 0; mt < 7; mt++)
                            {
                                if (mat.MaterialTextures[mt].TextureId != 212676728 && mat.MaterialTextures[mt].TextureId != 1509989155)
                                {
                                    xconvMat.MaterialTextures[mt] = mat.MaterialTextures[mt];
                                    try
                                    {
                                        xconvMat.MaterialTextures[mt].TextureId = modTexDb.GetTextureInfo(xTextureDatabase.GetTextureInfo(xconvMat.MaterialTextures[mt].TextureId).Name).Id;
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            xconvMat.ShaderName = "ITEM";
                        }
                        obj.Materials[m] = xconvMat;
                    }

                    if (obj.Skin != null)
                    {
                        Skeleton? xSkeleton = xBoneData.Skeletons.Find(x => x.Name == objsetName.Substring(0, 3));
                        Skeleton? mmSkeleton = mmBoneData.Skeletons.Find(x => x.Name == objsetName.Substring(0, 3));

                        if (xSkeleton == null || mmSkeleton == null)
                        {
                            if (xBoneData.Skeletons.Count > 0 || mmBoneData.Skeletons.Count > 0)
                            {
                                xSkeleton = xBoneData.Skeletons[0];
                                mmSkeleton = mmBoneData.Skeletons[0];
                                if (xSkeleton == null || mmSkeleton == null)
                                {
                                    throw new InvalidDataException($"Skeleton for {objsetName.Substring(0, 3)} not in both bone datas, and CMN could not be located. Aborting.");
                                }
                            }
                            else
                            {
                                throw new InvalidDataException($"A required bone data file contains no Skeletons. How is this possible? Aborting.");
                            }
                        }

                        foreach (var bone in obj.Skin.Bones)
                        {
                            if (!bone.IsEx)
                            {
                                bone.Id = (uint)mmSkeleton.ObjectBoneNames.IndexOf(bone.Name);
                            }
                        }

                        List<IBlock> newBlocks = new List<IBlock>();

                        if (obj.Skin != null)
                        {
                            foreach (var block in obj.Skin.Blocks)
                            {
                                if (block is MotionBlock motBlock)
                                {
                                    Console.WriteLine(motBlock.Name.IndexOf("_mot"));

                                    string motType = motBlock.Name.Substring(2, motBlock.Name.IndexOf("_mot") - 2);
                                    Dictionary<string, List<string>> osageChains;
                                    switch (motType)
                                    {
                                        case "cloth_skirt":
                                            osageChains = motClothSkirtOsageChains;
                                            break;
                                        case "cloth_long":
                                            osageChains = motClothLongOsageChains;
                                            break;
                                        case "hair_twin":
                                            osageChains = motHairTwinOsageChains;
                                            osageSetting.Objects.Add(new OsageSettingObject() { Name = obj.Name, Category = "long_tail_f" });
                                            break;
                                        case "hair_long":
                                            osageChains = motHairLongOsageChains;
                                            osageSetting.Objects.Add(new OsageSettingObject() { Name = obj.Name, Category = "long_hair_f" });
                                            break;
                                        default:
                                            throw new InvalidDataException($"Unknown mot block type {motType}");
                                    }
                                    foreach (string osageNodeRoot in osageChains.Keys)
                                    {
                                        MikuMikuLibrary.Objects.BoneInfo? bone = obj.Skin.GetBoneInfoByName(osageNodeRoot);
                                        int chainStartOffset = 0;
                                        while (chainStartOffset < osageChains[osageNodeRoot].Count)
                                        {
                                            string osgNodeRootInc = $"{osageNodeRoot.Substring(0, osageNodeRoot.IndexOf("000_wj"))}{chainStartOffset:D3}_wj";

                                            bone = obj.Skin.GetBoneInfoByName(osgNodeRootInc);

                                            if (bone == null)
                                            {
                                                Console.WriteLine($"Failed to get bone {osageNodeRoot.Substring(0, osageNodeRoot.IndexOf("000_wj"))}{chainStartOffset:D3}_wj. Attempting to locate bone {osageNodeRoot.Substring(0, osageNodeRoot.IndexOf("000_wj"))}{chainStartOffset + 1:D3}_wj");
                                                chainStartOffset++;
                                            }
                                            else
                                            {
                                                if (bone.Parent == null)
                                                {
                                                    bone.Parent = obj.Skin.GetBoneInfoByName(motBlock.ParentName);
                                                }
                                                OsageBlock osgBlock = new OsageBlock();

                                                osgBlock.ParentName = bone.Parent.Name;
                                                osgBlock.Name = $"e_{bone.Name.Substring(2, bone.Name.IndexOf($"_{chainStartOffset:D3}_wj") - 2)}";
                                                osgBlock.ExternalName = $"c_{bone.Name.Substring(2, bone.Name.IndexOf($"_{chainStartOffset:D3}_wj") - 2)}_osg";

                                                if (osageSkinParameterMap.TryGetValue(osageNodeRoot, out OsageSkinParameter? value))
                                                {
                                                    osp.Parameters.Add(value);

                                                    osp.Parameters[^1].Nodes.RemoveRange(0, chainStartOffset);
                                                }

                                                Matrix4x4.Invert(bone.InverseBindPoseMatrix, out var bindPoseMatrix);
                                                Matrix4x4 matrix = Matrix4x4.Multiply(bindPoseMatrix,
                                                    bone.Parent?.InverseBindPoseMatrix ?? Matrix4x4.Identity);

                                                Matrix4x4.Decompose(matrix, out Vector3 scl, out Quaternion rot, out Vector3 trs);

                                                rot = Quaternion.Normalize(rot);

                                                osgBlock.Position = matrix.Translation;
                                                osgBlock.Rotation = rot.ToEulerAngles();
                                                osgBlock.Scale = scl;

                                                if (osageExpHelpers.ContainsKey(osgNodeRootInc))
                                                {
                                                    ExpressionBlock exp = new ExpressionBlock();
                                                    exp.Position = matrix.Translation;
                                                    exp.Scale = Vector3.One;
                                                    exp.Name = $"n_{bone.Name.Substring(2, bone.Name.IndexOf($"_{chainStartOffset:D3}_wj") - 2)}_ex";
                                                    exp.ParentName = osgBlock.ParentName;
                                                    exp.Expressions.AddRange(osageExpHelpers[osageNodeRoot]);
                                                    osgBlock.Position = Vector3.Zero;
                                                    osgBlock.ParentName = exp.Name;
                                                    newBlocks.Add(exp);
                                                }

                                                foreach (string chainMember in osageChains[osageNodeRoot].Skip(chainStartOffset))
                                                {
                                                    OsageNode chainNode = new OsageNode();
                                                    chainNode.Name = chainMember;
                                                    chainNode.Length = osageNodeLengths[chainMember];
                                                    osgBlock.Nodes.Add(chainNode);
                                                }

                                                // because twin hair is broken ig, copied values from MFMK
                                                switch (osgBlock.Name)
                                                {
                                                    case "e_hair_twin_l":
                                                        osgBlock.Rotation = new Vector3(2.792527f, 1.570796f, 0);
                                                        break;
                                                    case "e_hair_twin_r":
                                                        osgBlock.Rotation = new Vector3(0.3490656f, 1.570796f, 0);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                newBlocks.Add(osgBlock);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            obj.Skin?.Blocks.RemoveAll(x => x.Signature == "MOT");
                            obj.Skin?.Blocks.AddRange(newBlocks);
                            if (osp.Parameters.Count != 0)
                                osp.Save(Path.Combine(args[2], "rom", "skin_param", $"ext_skp_{obj.Name}.txt"));
                        }

                    }
                }

                modObjDb.ObjectSets.Add(setInfo);

                xObjectSet.Format = BinaryFormat.DT;
                xTextureSet.Format = BinaryFormat.DT;

                using (FarcArchive farc = new FarcArchive())
                {
                    if (objsetName.EndsWith("ITM000"))
                    {
                        farc.Add($"{objsetName.ToLower()}_obj.bin", xObjectSet);
                        farc.Add($"{objsetName.ToLower()}_tex.bin", xTextureSet);

                        farc.Save(Path.Combine(args[2], "rom", "objset", $"{objsetName.ToLower()}.farc"));
                    }
                    else
                    {
                        farc.Add($"{objsetName.ToLower()}_x_obj.bin", xObjectSet);
                        farc.Add($"{objsetName.ToLower()}_x_tex.bin", xTextureSet);

                        farc.Save(Path.Combine(args[2], "rom", "objset", $"{objsetName.ToLower()}_x.farc"));
                    }
                }

                objsetBaseId++;
            }

            modObjDb.Save(Path.Combine(args[2], "rom", "objset", "mod_obj_db.bin"));
            modTexDb.Save(Path.Combine(args[2], "rom", "objset", "mod_tex_db.bin"));

            // Convert relevant motion sets
            
            for (int i = 0; i < (int)CharacterType.EXT; i++)
            {
                string charName = ((CharacterType)i).ToString();
                Skeleton mmCharSkel = mmBoneData.Skeletons.First(x => x.Name == charName);
                Skeleton xCharSkel = xBoneData.Skeletons.First(x => x.Name == charName);

                MotionSetInfo mmMotSetInfo = mmMotionData.GetMotionSetInfo(charName);

                MotionSet mmMotSet = cpk.Open<FarcArchive>($"rom/rob/mot_{charName}.farc").Open($"mot_{charName}.bin", stream =>
                {
                    MotionSet motSet = new MotionSet();
                    motSet.Load(stream, mmCharSkel, mmMotionData);
                    return motSet;
                });

                FarcArchive xFaceMotArc = null;


                ACCTPath motPathEntry = firstRead.AddonContentContainers[0].Paths.First(x => x.FileName == $"mot_{charName.ToLower()}.farc");

                if (motPathEntry.Flags == ACCTPathMode.Packed)
                {
                    if (arc != null)
                    {
                        xFaceMotArc = BinaryFile.Load<FarcArchive>(arc.Open($"mot_{charName.ToLower()}.farc"));
                    }
                    else
                    {
                        xFaceMotArc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], "data", $"mot_{charName.ToLower()}.farc"));
                    }
                }
                else
                {
                    xFaceMotArc = BinaryFile.Load<FarcArchive>(Path.Combine(args[0], motPathEntry.FilePath, motPathEntry.FileName));

                }

                foreach (var fileName in xFaceMotArc.FileNames)
                {
                    if (fileName.EndsWith(".mot"))
                    {
                        Motion xFaceMot = xFaceMotArc.Open<Motion>(fileName, stream =>
                        {
                            Motion mot = new Motion();
                            mot.Load(stream, xCharSkel);
                            return mot;
                        });

                        for (int m = 0;  m < mmMotSet.Motions.Count; m++)
                        {
                            if (mmMotSetInfo.Motions[m].Name.Contains(xFaceMot.Name))
                                mmMotSet.Motions[m] = xFaceMot;
                        }
                    }
                }

                using (FarcArchive motFarc = new FarcArchive())
                {
                    MemoryStream mmMotStream = new MemoryStream();
                    mmMotSet.Save(mmMotStream, mmCharSkel, mmMotionData, true);
                    motFarc.Add($"mot_{charName}.bin", mmMotStream, false);

                    motFarc.Save(Path.Combine(args[2], "rom", "rob", $"mot_{charName}.farc"));
                }
            }

            ParameterTreeWriter osageSettingWriter = new ParameterTreeWriter();
            osageSettingWriter.Write("cat", osageSetting.Categories, x =>
            {
                osageSettingWriter.Write("name", x.Name);
                osageSettingWriter.Write("osg", x.Osg, o =>
                {
                    osageSettingWriter.Write("exf", o.EXF);
                    osageSettingWriter.Write("parts", o.Parts.ToString());
                    osageSettingWriter.Write("root", o.Root);
                });
            });

            osageSettingWriter.Write("obj", osageSetting.Objects, x =>
            {
                osageSettingWriter.Write("cat", x.Category);
                osageSettingWriter.Write("name", x.Name.ToUpper());
            });

            osageSettingWriter.Save(Path.Combine(args[2], "rom", "skin_param", "mod_osage_setting.txt"));
        }

        langToml.Close();
    }
}