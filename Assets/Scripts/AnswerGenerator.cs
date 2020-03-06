﻿using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using Unity;
using UnityEngine;

class AnswerGenerator
{
    //正解入力候補リスト
    public List<List<string>> AnswerRomajiInputSpellingList { get; private set; }
    //UI表示用問題テキスト
    public string QuestionText { get; private set; }
    //UI表示に使うかもしれない、問題のひらがな表記
    public string QuestionKanaSpelling { get; private set; }
    //出題文字列を文字ごとに切ったリスト
    public List<string> CharList { get; private set; }
    public Dictionary<string, string[]> RomajiKanaMap { get; private set; }
    private static int DataNum = 0;
    //重複した出題を防ぐための、既出問題インデックスリスト
    private static List<int> UsedRows = new List<int>();

    public AnswerGenerator(string jsonFilePath, string dbPath, string tableName)
    {
        //ローマ字から変換辞書を引数のjsonから辞書型に変換して生成
        RomajiKanaMap = GenerateKanaMapDictionary(jsonFilePath);
        SelectQuestion(dbPath, tableName);
        List<string> CharList = ParseHiraganaSentence(QuestionKanaSpelling);
        Debug.Log(QuestionKanaSpelling);
        //データベースから取得したかな文字列から入力候補リストを生成
        AnswerRomajiInputSpellingList = ConstructSentence(CharList);
    }

    //指定されたjsonファイル(パス)からローマ字かな変換用辞書を作成
    private Dictionary<string, string[]> GenerateKanaMapDictionary(string jsonFilePath)
    {
        string jsonString = File.ReadAllText(jsonFilePath);
        var values = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonString);
        return values;
    }

    //SQLコマンド(ExecuteReader用)を引数に取り、所定のデータベースとトランザクションをする
    private void SelectQuestion(string dbPath, string tableName)
    {
        int row;

        var dbp = new SQLiteConnectionStringBuilder { DataSource = dbPath };
        using (var cn = new SQLiteConnection(dbp.ToString()))
        {
            cn.Open();
            using (var command = new SQLiteCommand(cn))
            {
                //データ数が未定義（初めの１回）のときのみデータ数を定義
                if (DataNum == 0)
                {
                    command.CommandText = "SELECT COUNT() FROM " + tableName;
                    using (SQLiteDataReader returnedSdr = command.ExecuteReader())
                    {
                        foreach (var _ in returnedSdr)
                        {
                            DataNum = returnedSdr.GetInt32(0);
                        }
                        //Console.WriteLine(DataNum);
                    }

                }

                //重複しないように問題選択重複しないように問題選択
                do
                {
                    var r = new System.Random();

                    row = r.Next(1, DataNum + 1);
                } while (UsedRows.Contains(row));
                UsedRows.Add(row);
                command.CommandText = "select text,kana from " + tableName + " limit 1 offset " + (row - 1).ToString();
                using (SQLiteDataReader returnedSdr = command.ExecuteReader())
                {
                    foreach (var _ in returnedSdr)
                    {
                        QuestionText = returnedSdr.GetString(0);
                        QuestionKanaSpelling = returnedSdr.GetString(1);
                    }
                    //Console.WriteLine("QT : "+QuestionText);
                    //Console.WriteLine("QS : "+QuestionKanaSpelling);
                }
            }
        }
    }


    //正解かな文字列から文字ごとに区切り、リストを生成
    private List<string> ParseHiraganaSentence(string str)
    {
        //返す文字（「ちゃ」など複数文字の場合に備えてリスト）
        var ret = new List<string>();
        //かな文字列リスト中で現在見ている文字の位置
        int i = 0;
        //uni:i文字目、bi:i文字目＋i+1文字目
        string uni, bi;
        while (i < str.Length)
        {
            uni = str[i].ToString();
            //末尾以外のかなには一度biを生成・末尾のbiは空文字列(以下ですべての場合にbiをチェックするため、biがなくても生成しておく)
            if (i + 1 < str.Length)
            {
                bi = str[i].ToString() + str[i + 1].ToString();
            }
            else
            {
                bi = "";
            }
            
            //一度生成したbiが辞書にヒットしたら区切りを確定・かな単位リストにbiで登録してインデックスを進める、ヒットしなければuniで登録し、一つずらして再チェック(末尾の文字はbiが空文字列なので必ずヒットせずuniが登録される)
            if (RomajiKanaMap.ContainsKey(bi))
            {
                i += 2;
                ret.Add(bi);
            }
            else
            {
                i++;
                ret.Add(uni);
            }
        }
        return ret;
    }

    //かなリストの各要素（各単位かな毎）の正解ローマ字リストを生成
    private List<List<string>> ConstructSentence(List<string> str)
    {
        //返すローマ字リストのリスト
        var ret = new List<List<string>>();
        //現在・および次のかな
        string s, ns;
        //入力された各仮名について
        for (int i = 0; i < str.Count; ++i)
        {
            //i文字目のかなをsに
            s = str[i];
            //str[i]が末尾でなければ次の文字をnsに、末尾ならnsは空にする
            if (i + 1 < str.Count)
            {
                ns = str[i + 1];
            }
            else
            {
                ns = "";
            }
            //暫定版正解ローマ字リスト
            var tmpList = new List<string>();
            // ん の処理
            if (s.Equals("ん"))
            {
                //n一回の入力で代替できるか
                bool isValidSingleN;
                //ローマ字かな変換表における、その仮名に対応するローマ字の暫定リスト
                var nList = RomajiKanaMap[s];
                // 文末の「ん」-> nn, xn のみ
                if (str.Count - 1 == i)
                {
                    isValidSingleN = false;
                }
                // 後ろに母音, ナ行, ヤ行 -> nn, xn のみ
                else if (i + 1 < str.Count && (ns.Equals("あ") || ns.Equals("い") || ns.Equals("う") || ns.Equals("え") || ns.Equals("お") || ns.Equals("な") || ns.Equals("に") || ns.Equals("ぬ") || ns.Equals("ね") || ns.Equals("の") || ns.Equals("や") || ns.Equals("ゆ") || ns.Equals("よ")))
                {
                    isValidSingleN = false;
                }
                // それ以外は n も許容
                else
                {
                    isValidSingleN = true;
                }
                foreach (var t in nList)
                {
                    if (!isValidSingleN && t.Equals("N"))
                    {
                        continue;
                    }
                    tmpList.Add(t);
                }
                //Debug.Log(s+", "+i+", "+isValidSingleN);
            }
            // っ の処理
            else if (s.Equals("っ"))
            {
                //「っ」自体
                var smallTsu = RomajiKanaMap[s];
                //その直後の文字
                var nsList = RomajiKanaMap[ns];
                var doubleNextSet = new HashSet<string>();
                // 次の文字の子音だけとってくる
                foreach (string t in nsList)
                {
                    //※t[0]:次のかなの各打ち方のうち1文字目
                    string c = t[0].ToString();
                    doubleNextSet.Add(c);
                }
                var doubleNextList = doubleNextSet.ToList();
                List<string> smallTsulTypeList = doubleNextList.Concat(smallTsu).ToList();
                tmpList = smallTsulTypeList;
            }
            // ちゃ などのように tya, cha や ち + ゃ を許容するパターン。sが二文字（一気にかな2文字分入力）で1文字目が「ん」でない
            else if (s.Length == 2 && !Equals("ん", s[0]))
            {
                // ちゃ などとそのまま打つパターンの生成
                tmpList = tmpList.Concat(RomajiKanaMap[s]).ToList();
                // ち + ゃ などの分解して入力するパターンを生成
                var fstList = RomajiKanaMap[s[0].ToString()];
                var sndList = RomajiKanaMap[s[1].ToString()];
                var retList = new List<string>();
                //1文字目・2文字目の各パターンを全パターン接続して1つのリストにまとめる
                foreach (string fstStr in fstList)
                {
                    foreach (string sndStr in sndList)
                    {
                        string t = fstStr + sndStr;
                        retList.Add(t);
                    }
                }
                tmpList = tmpList.Concat(retList).ToList();
            }
            // それ以外
            else
            {
                tmpList = RomajiKanaMap[s].ToList();
            }
            ret.Add(tmpList);
        }
        return ret;
    }
}
