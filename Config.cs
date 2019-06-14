﻿using Newtonsoft.Json;
using System;

namespace CsAsODS
{
    public class Config
    {
        public General General { get; set; } = new General();
        public GeoData GeoData { get; set; } = new GeoData();
        public SQLData SQLData { get; set; } = new SQLData();
    }
    public class General
    {
        public string Lang { get; set; } = "zh-CN";
        public bool Hex { get; set; } = false;
        public bool LogIO { get; set; } = true;
    }

    public class GeoData
    {
        public bool Enable { get; set; } = true;
        public string IPInput { get; set; } = "IPInput.txt";
        public string IPOutput { get; set; } = "IPOutput.txt";
        public string[] IPBackFormat { get; set; } = {
            "countryCode",
            "country",
            "regionName",
            "city"
        };
    }
    public class SQLData
    {
        public bool Enable { get; set; } = false;
        public string Server { get; set; } = "localhost";
        public string Port { get; set; } = "3306";
        public string Prefix { get; set; } = "SvenCoop";
        public string Database { get; set; } = "Ecco";
        public string Account { get; set; } = "root";
        public string Password { get; set; } = "secret";
        public string SQLInput { get; set; } = "SQLInput.txt";
        public string SQLOutput { get; set; } = "SQLOutput.txt";
        public string SQLChangeput { get; set; } = "SQLChangeput.txt";
    }

    public class ConfData
    {
        public static Config conf = new Config();
        public static bool JsReader()
        {
            string json = Reader.g_Reader.JsonReader(Program.FileDir + "config.json");
            if (string.IsNullOrEmpty(json))
            {
                CCUtility.g_Utility.CritWarn(
                    "不存在配置文件，将生成默认配置文件！\n" +
                    "There is no configuration file, the default configuration file will be generated!\n");
                CreateJson();
                return true;

            }
            else
            {
                try
                {
                    conf = JsonConvert.DeserializeObject<Config>(json);
                    return true;
                }
                catch (Exception e)
                {
                    CCUtility.g_Utility.CritError(
                        "文件格式不正确！无法读取配置文件！请检查json文件拼写！\n" +
                        "The file format is incorrect! Unable to read the configuration file! Please check the spelling of JSON file!\n" ,
                        "错误代码/Error Code: " + e.Message.ToString() + "\n");
                    return false;
                }
            }
        }
        public static void CreateJson()
        {
            CCUtility.g_Utility.Warn("将生成默认配置文件\nDefault Configuration files will be generated");
            string dconf = JsonConvert.SerializeObject(conf);
            CCWriter.g_Writer.Writer(Program.FileDir + "config.json", dconf);
            CCUtility.g_Utility.Succ("默认配置文件生成完毕\nThe default configuration file has been generated");
        }
    }
}