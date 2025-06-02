using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TS_Server.DataTools
{
    public static class WarpData
    {
        public static Dictionary<ushort, Dictionary<ushort, ushort[]>> warpList = new Dictionary<ushort, Dictionary<ushort, ushort[]>>();
        public static int warpCount = 0;

        //public static void loadDat()
        //{
        //    {
        //        String FileName = "Warp.Dat";
        //        int iBlock = 23;

        //        var buffer = getBuffer();
        //        if (buffer != null)
        //        {
        //            int iTotalBlock = buffer.Length / iBlock;

        //            for (int i = 1; i < iTotalBlock; i++)
        //            {
        //                TsOnline.Entity.WarpEntity item = new TsOnline.Entity.WarpEntity();

        //                int index = i * iBlock;
        //                item.LengthName = buffer[index++];
        //                Array.Reverse(buffer, index, 10);
        //                sheet.Cells[i + 1, 1].Value = item.Name = Encoding.Default.GetString(buffer, index, item.LengthName).Trim();
        //                index += 10;
        //                sheet.Cells[i + 1, 2].Value = item.b1 = ProgressByte(buffer[index++]);
        //                sheet.Cells[i + 1, 3].Value = item.b2 = ProgressByte(buffer[index++]);
        //                sheet.Cells[i + 1, 4].Value = item.u1 = ProgressUshort(BitConverter.ToUInt16(buffer, index));
        //                index += 2;
        //                sheet.Cells[i + 1, 5].Value = item.i1 = ProgressInt(BitConverter.ToInt32(buffer, index));
        //                index += 4;
        //                sheet.Cells[i + 1, 6].Value = item.i2 = ProgressInt(BitConverter.ToInt32(buffer, index));
        //                index += 4;

        //                listData.Add(item);
        //            }
        //        }

        //    }

        //    private byte ProgressByte(byte input)
        //    {
        //        return (byte)((input ^ 0xC8) - 1);
        //    }
        //    private ushort ProgressUshort(ushort input)
        //    {
        //        return (ushort)((input ^ 0x5209) - 1);
        //    }
        //    private int ProgressInt(int input)
        //    {
        //        return (int)((input ^ 0x0BAEB716) - 1);
        //    }
        //}

        public static void loadTxt(string input)
        {
            FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read);
            StreamReader s = new StreamReader(fs);
            ushort map1, warpId, map2, x, y;
            s.ReadLine();
            while (!s.EndOfStream)
            {
                string str = s.ReadLine();
                string[] data = Regex.Split(str, @"\D+");
                map1 = ushort.Parse(data[0]);
                warpId = ushort.Parse(data[1]);
                map2 = ushort.Parse(data[2]);
                x = ushort.Parse(data[3]);
                y = ushort.Parse(data[4]);
                addGateway(map1, warpId, map2, x, y);
            }
            s.Close(); fs.Close();
        }

        public static void addGateway(ushort map1, ushort warpId, ushort map2, ushort x, ushort y)
        {
            ushort[] dest = new ushort[3] {map2, x, y};

            if (!warpList.ContainsKey(map1))
            {
                Dictionary<ushort, ushort[]> warpinfo = new Dictionary<ushort, ushort[]>();
                warpinfo.Add(warpId, dest);
                warpList.Add(map1, warpinfo);
                warpCount++;
            }
            else
                if (!warpList[map1].ContainsKey(warpId))
                {
                    warpList[map1].Add(warpId, dest);
                    warpCount++;
                }
                else
                    warpList[map1][warpId] = dest;
        }

        public static void loadWarpEx()
        {
            FileStream fs = new FileStream("door.ini", FileMode.Open, FileAccess.Read);
            StreamReader s = new StreamReader(fs);
            ushort map1, warpId, map2, x, y;
            while (!s.EndOfStream)
            {
                string str = s.ReadLine();
                string[] data = Regex.Split(str, @"\D+");

                try
                {
                    map1 = ushort.Parse(data[0]);
                    warpId = ushort.Parse(data[1]);
                    map2 = ushort.Parse(data[2]);

                    //Console.WriteLine("trying map " + map1);
                    Tuple<ushort, ushort> coor = EveData.loadCoor(map1, map2, warpId);
                    if (coor != null)
                    {
                        x = coor.Item1;
                        y = coor.Item2;
                        addGateway(map1, warpId, map2, x, y);
                    }
                   // else
                       // Console.WriteLine("warp " + map1 + " " + map2 + " not found");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            s.Close(); fs.Close();
        }

        public static void loadWarpDoDo()
        {
            FileStream fs = new FileStream("WarpID.ini", FileMode.Open, FileAccess.Read);
            StreamReader s = new StreamReader(fs);
            ushort map1, warpId, map2, x, y;
            while (!s.EndOfStream)
            {
                string str = s.ReadLine();
                string[] data = Regex.Split(str, @"\D+");

                try
                {
                    map1 = ushort.Parse(data[1]);
                    warpId = ushort.Parse(data[2]);
                    map2 = ushort.Parse(data[3]);

                    //Console.WriteLine("trying map " + map1);
                    Tuple<ushort, ushort> coor = EveData.loadCoor(map1, map2, warpId);
                    if (coor != null)
                    {
                        x = coor.Item1;
                        y = coor.Item2;
                        addGateway(map1, warpId, map2, x, y);
                       
                    }
                    //else
                        //Console.WriteLine("warp " + map1 + " " + map2 + " not found");
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            s.Close(); fs.Close();
        }

        public static void writeToFile(string output)
        {
            FileStream fs = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter s = new StreamWriter(fs);

            s.Write("map1\t");
            s.Write("warpid\t");
            s.Write("map2\t");
            s.Write("x\t");
            s.Write("y\n");

            foreach (ushort i in warpList.Keys)
            {
                foreach (ushort j in warpList[i].Keys)
                {
                    s.Write(i + "\t");
                    s.Write(j + "\t");
                    s.Write(warpList[i][j][0] + "\t");
                    s.Write(warpList[i][j][1] + "\t");
                    s.Write(warpList[i][j][2] + "\n");                    
                }
            }

            s.Close();
            fs.Close();

        }
    }
}
