using Assimp.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuLibrary.IO.Common;
using System.IO.Compression;

namespace XRomConverter
{
    [Flags]
    public enum ArchiveFlags
    {
        IgnoreCase = 1,
        AbsolutePaths = 2
    }

    public class PSARCTocEntry
    {
        public Byte[] MD5Hash;
        public uint BlockIndex;
        public long FileSize;
        public long FileOffset;

        public void Read(EndianBinaryReader reader)
        {
            MD5Hash = reader.ReadBytes(16);

            BlockIndex = reader.ReadUInt32();

            byte[] fileSizeBytes = new byte[3];
            fileSizeBytes = fileSizeBytes.Concat(reader.ReadBytes(5)).ToArray();
            byte[] fileOffsetBytes = new byte[3];
            fileOffsetBytes = fileOffsetBytes.Concat(reader.ReadBytes(5)).ToArray();
            FileSize = BitConverter.ToInt64(fileSizeBytes.Reverse().ToArray());
            FileOffset = BitConverter.ToInt64(fileOffsetBytes.Reverse().ToArray());


        }

    }

    public class PSARC
    {
        public int Version { get; set; }
        public string CompressionType { get; set; } = "zlib";
        private int mEntrySize;
        public List<PSARCTocEntry> Files { get; set; }
        private int mNumBlocks;
        private int mBlockSize;
        public ArchiveFlags Flags { get; set; }
        private ushort[]? mBlockSizes;
        private Stream? mBaseStream;
        public List<string> FileNames { get; set; }

        public void Load(string filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.OpenRead(filePath), System.Text.Encoding.UTF8, MikuMikuLibrary.IO.Endianness.Big, true))
            {
                char[] magic = reader.ReadChars(4);
                if (magic[0] != 'P' || magic[1] != 'S' ||  magic[2] != 'A' || magic[3] != 'R')
                {
                    throw new InvalidDataException("Not valid PSARC or Encrypted.");
                }

                Version = reader.ReadInt32();

                CompressionType = new string(reader.ReadChars(4));

                int headerSize = reader.ReadInt32();

                mEntrySize = reader.ReadInt32();
                int numFiles = reader.ReadInt32();
                Files.Capacity = numFiles;
                mBlockSize = reader.ReadInt32();
                Flags = (ArchiveFlags)reader.ReadInt32();

                for (int i = 0; i < numFiles; i++)
                {
                    PSARCTocEntry entry = new PSARCTocEntry();
                    entry.Read(reader);
                    Files.Add(entry);
                }

                long curPos = reader.Position;
                long numBlocks = (headerSize - curPos) / 2;
                mBlockSizes = new ushort[numBlocks];
                for (int i = 0; i < numBlocks; i++)
                {
                    mBlockSizes[i] = reader.ReadUInt16();
                }

                mBaseStream = reader.BaseStream;

                StreamReader manReader = new StreamReader(Open(0));

                string? fileName;
                while (true)
                {
                    fileName = manReader.ReadLine();
                    Console.WriteLine(fileName);
                    if (fileName == null)
                    {
                        break;
                    }
                    FileNames.Add(fileName);
                }
            }
        }

        public MemoryStream Open(string fileName)
        {
            if (FileNames.Contains(fileName))
                return Open(FileNames.IndexOf(fileName) + 1);
            else
                throw new FileNotFoundException($"No file {fileName} in archive.");
        }

        public MemoryStream Open(int fileIndex)
        {
            PSARCTocEntry file = Files[fileIndex];

            mBaseStream.Seek(file.FileOffset, SeekOrigin.Begin);

            uint curBlock = file.BlockIndex;
            int decompressed = 0;
            byte[] decDat = new byte[file.FileSize];
            bool nextDecompressed = false;
            while (decompressed < file.FileSize)
            {
                if (mBlockSizes[curBlock] != 0)
                {
                    if (mBlockSizes[curBlock] + decompressed != file.FileSize)
                    {
                        byte[] cmpDat = new byte[mBlockSizes[curBlock]];
                        mBaseStream.Read(cmpDat, 0, mBlockSizes[curBlock]);
                        MemoryStream cmpStream = new MemoryStream(cmpDat, 2, mBlockSizes[curBlock] - 2);
                        DeflateStream def = new DeflateStream(cmpStream, CompressionMode.Decompress);
                        int read;
                        while (true)
                        {
                            read = def.Read(decDat, decompressed, (int)(file.FileSize - decompressed));
                            if (read == 0)
                            {
                                break;
                            }
                            decompressed += read;
                        }
                        def.Close();
                    }
                    else
                    {
                        int readSize = mBlockSizes[curBlock] == 0 ? mBlockSize : mBlockSizes[curBlock];
                        mBaseStream.Read(decDat, decompressed, readSize);
                        decompressed += mBlockSize;
                    }
                }
                else
                {
                    int readSize = mBlockSizes[curBlock] == 0 ? mBlockSize : mBlockSizes[curBlock];
                    mBaseStream.Read(decDat, decompressed, readSize);
                    decompressed += mBlockSize;
                }
                curBlock += 1;
            }

            return new MemoryStream(decDat);
        }

        public PSARC()
        {
            mBaseStream = null;
            mBlockSizes = null;
            Files = new List<PSARCTocEntry>();
            FileNames = new List<string>();
        }
    }
}
