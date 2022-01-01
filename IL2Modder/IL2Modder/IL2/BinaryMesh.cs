using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace IL2Modder.IL2
{
    public struct Table1Entry
    {
        public Int32 Type;
        public Int32 Guess;
        public Int16 NumberOfRecords;
        public Int16 SomethingElse;
    };
    public class BinaryMesh
    {
        Int32 header0;
        Int32 header1;
        public Int32 NumberOfSections;
        public Int32 header3;
        public Int32 Table1start;
        public Int32 header5;
        public Int32 Table2start;
        public byte[] byte_buffer;
        public byte[] table2;
        long pos;

        public List<String> contents = new List<string>();
        public List<Table1Entry> table1 = new List<Table1Entry>();
        public List<int> table3 = new List<int>();

        public BinaryMesh(String file)
        {
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                header0 = b.ReadInt32();
                header1 = b.ReadInt32();
                NumberOfSections = b.ReadInt32();
                header3 = b.ReadInt32();
                Table1start = b.ReadInt32();
                header5 = b.ReadInt32();
                Table2start = b.ReadInt32();

                for (int i = 0; i < NumberOfSections; i++)
                {
                    contents.Add(ReadString(b));
                }

                pos = b.BaseStream.Position;
                long bytes_left = b.BaseStream.Length - pos;
                byte_buffer = b.ReadBytes((int)bytes_left);

                b.Close();
                ReadTable1(file);
                ReadTable2(file);
                ReadTable3(file);

            }
            
        }
        private String ReadString(BinaryReader reader)
        {
            String result;
            byte nchars = reader.ReadByte();
            byte[] chars = new byte[nchars];
            for (int i = 0; i < nchars; i++)
            {
                chars[i] = reader.ReadByte();
            }
            result = System.Text.Encoding.Default.GetString(chars);
            return result;
        }
        public int FindShort(Int32[] targets, int count)
        {
            int i = 0;
            int j = 0;
            while (i < byte_buffer.GetLength(0))
            {
                byte t1 = (byte)(targets[j / 2] & 255);
                byte t2 = (byte)((targets[j / 2] >> 8) & 255);
                if ((t1 == byte_buffer[i + j]) && (t2 == byte_buffer[i + j + 1]))
                {
                    j += 2;
                    if ((j / 2) == count)
                    {
                        return i;
                    }
                }
                else
                {
                    j = 0;
                    i++;
                }
            }
            return 0;
        }
        public int FindFloat(float[] targets, int count)
        {
            
            return 0;
        }
        

        private void ReadTable1(String file)
        {
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                b.BaseStream.Position = Table1start;
                for (int i = 0; i < NumberOfSections; i++)
                {
                    Table1Entry t = new Table1Entry();
                    t.Type = b.ReadInt32();
                    t.Guess = b.ReadInt32();
                    t.NumberOfRecords = b.ReadInt16();
                    t.SomethingElse = b.ReadInt16();
                    table1.Add(t);
                }
                b.Close();
            }
        }
        private void ReadTable2(String file)
        {
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                b.BaseStream.Position = header5;
                int size = Table2start - header5;
                table2 = b.ReadBytes(size);
                b.Close();
            }
        }
        private void ReadTable3(String file)
        {
            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                b.BaseStream.Position = Table2start;
                int end = b.ReadInt32();
                while (b.BaseStream.Position < end)
                {
                    table3.Add(b.ReadInt32());
                }
                b.Close();
            }
        }
    }
}
