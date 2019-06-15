﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsAsODS
{
    class JsonSQL
    {
        FileSystemWatcher fsw = null, fswc = null;
        public void Stop()
        {
            fsw.EnableRaisingEvents = false;
            fsw.Dispose();
            fswc.EnableRaisingEvents = false;
            fswc.Dispose();
        }
        public void SQLWatcher()
        {
            CCUtility.g_Utility.Succ(LangData.lg.SQL.Running + ": " + ConfData.conf.SQLData.SQLType);
            string JsonFile = Program.FileDir + "SqlJson/" + ConfData.conf.SQLData.Database + ".json";
            Dictionary<string, JsonCollection> JsonData = new Dictionary<string, JsonCollection>();
            if (!Directory.Exists(Program.FileDir + "SqlJson") || !File.Exists(JsonFile))
            {
                CCUtility.g_Utility.Warn(LangData.lg.SQL.FirstRun);
                Directory.CreateDirectory(Program.FileDir + "/SqlJson");
                CCWriter.g_Writer.Writer(JsonFile, JsonConvert.SerializeObject(JsonData));
            }
            else
                JsonData = JsonConvert.DeserializeObject<Dictionary<string, JsonCollection>>(Reader.g_Reader.ReadIt(JsonFile));

            //监视文件
            fsw = new FileSystemWatcher
            {
                //获取程序路径
                Path = Program.FileDir,
                //获取或设置要监视的更改类型
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                //要监视的文件
                Filter = ConfData.conf.SQLData.SQLInput,
                //设置是否级联监视指定路径中的子目录
                IncludeSubdirectories = false
            };
            //添加事件
            fsw.Changed += OnChanged;
            // 开始监听
            fsw.EnableRaisingEvents = true;

            //监视文件
            fswc = new FileSystemWatcher
            {
                //获取程序路径
                Path = Program.FileDir,
                //获取或设置要监视的更改类型
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                //要监视的文件
                Filter = ConfData.conf.SQLData.SQLChangeput,
                //设置是否级联监视指定路径中的子目录
                IncludeSubdirectories = false
            };
            //添加事件
            fswc.Changed += OnUpdate;
            // 开始监听
            fswc.EnableRaisingEvents = true;

            void OnChanged(object source, FileSystemEventArgs e)
            {
                CCUtility.g_Utility.FileWatcherLog(e.Name + LangData.lg.SQL.Changed);
                Search();
            }

            void OnUpdate(object source, FileSystemEventArgs e)
            {
                string changePath = e.FullPath;
                CCUtility.g_Utility.FileWatcherLog(e.Name + LangData.lg.SQL.Changed);
                string str = Reader.g_Reader.ReadIt(changePath);
                string[] line = str.Split('\n');
                for (int i = 0; i < line.Length; i++)
                {
                    CCUtility.g_Utility.Taskbar(String.Format(LangData.lg.SQL.Remain, line.Length - i));
                    if (!string.IsNullOrEmpty(line[i]))
                    {
                        string[] sz = line[i].Split(',');
                        Update(sz[0], sz[1]);
                    }
                }
                CCUtility.g_Utility.Taskbar(LangData.lg.General.QuestFinish);
            }

            void Update(string szID, string szEcco)
            {
                if (JsonData.ContainsKey(szID))
                {
                    JsonCollection data = JsonData[szID];
                    data.Ecco = Convert.ToInt32(szEcco);
                    JsonData[szID] = data;
                }
                SaveIt();
            }

            //查询请求
            void Search()
            {
                string inPath = Program.FileDir + ConfData.conf.SQLData.SQLInput;
                string outPath = Program.FileDir + ConfData.conf.SQLData.SQLOutput;
                string[] line = Reader.g_Reader.ReadIt(inPath).Split(',');
                bool IsExs = false;
                string[] outLine = Reader.g_Reader.ReadIt(outPath).Split('\n');
                for (int i = 0; i < outLine.Length; i++)
                {

                    if (string.IsNullOrEmpty(outLine[i]))
                        continue;
                    else
                    {
                        string[] zj = outLine[i].Split(',');
                        if (zj[1] == line[0])
                        {
                            outLine[i] = Request(line[0], line[1]);
                            IsExs = true;
                        }
                    }
                }

                string op = "";
                for (int i = 0; i < outLine.Length; i++)
                {
                    if (!string.IsNullOrEmpty(outLine[i]))
                        op = op + outLine[i] + "\n";
                }
                if (!IsExs)
                    op = op + Request(line[0], line[1]);
                CCWriter.g_Writer.Writer(outPath, op);
            }

            string Request(string szID, string szNick)
            {
                try
                {
                    string szReturn = "";
                    if (JsonData.ContainsKey(szID))
                    {
                        JsonCollection data = JsonData[szID];
                        data.Nick = szNick;
                        JsonData[szID] = data;
                        szReturn = data.ID + "," + data.SteamID + "," + szNick + "," + data.Ecco;
                    }
                    else
                    {
                        JsonCollection data = new JsonCollection()
                        {
                            ID = JsonData.Count + 1,
                            SteamID = szID,
                            Nick = szNick,
                            Ecco = 0
                        };
                        JsonData[szID] = data;
                        szReturn = data.ID + "," + data.SteamID + "," + data.Nick + "," + data.Ecco;
                    }
                    SaveIt();
                    return szReturn;
                }
                catch (Exception e)
                {
                    CCUtility.g_Utility.Error(LangData.lg.SQL.RequstError + ": " + e.Message.ToString());
                }
                return null;
            }

            void SaveIt()
            {
                CCWriter.g_Writer.Writer(JsonFile, JsonConvert.SerializeObject(JsonData));
            }
        }
    }
    class JsonCollection
    {
        public int ID { get; set; } = 0;
        public string SteamID { get; set; } = "";
        public string Nick { get; set; } = "";
        public int Ecco { get; set; } = 0;
    }
}