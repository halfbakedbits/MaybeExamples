﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaybeAsAStruct
{
    class Program
    {
        static void Main(string[] args)
        {

        }
        
        static void Test2()
        {
            var contents = GetLogContents(1);

            if (contents.TryGetValue(out var value))
            {
                Console.WriteLine(value);
            }
            else
            {
                Console.WriteLine("Log file not found");
            }
        }

        static void Test3()
        {
            var contents = GetLogContents(1);

            contents.Match(some: value =>
            {
                Console.WriteLine(value);
            },
            none: () =>
            {
                Console.WriteLine("Log file not found");
            });
        }

        static void Test4()
        {
            var errorDescriptionMaybe =
                GetLogContents(13)
                    .Bind(contents => FindErrorCode(contents))
                    .Bind(errorCode => GetErrorDescription(errorCode));
        }

        static void Test5()
        {
            var errorDescriptionMaybe =
                GetLogContents(13)
                    .Bind(contents => FindErrorCode(contents)
                        .Bind(errorCode => GetErrorDescription(errorCode, contents)));
        }

        static void Test6()
        {
            var errorDescriptionMaybe =
                from contents in GetLogContents(13)
                from errorCode in FindErrorCode(contents)
                from errorDescription in GetErrorDescription(errorCode, contents)
                select errorDescription;
        }

        static void Test7()
        {
            var errorDescriptionMaybe =
                from contents in GetLogContents(13)
                from errorCode in FindErrorCode(contents)
                where errorCode < 1000
                from errorDescription in GetErrorDescription(errorCode, contents)
                select errorDescription;
        }

        static void Test8()
        {
            var errorMessage =
                GetErrorDescription(15)
                    .ValueOr("Unknown error");
        }

        static void Test9()
        {
            var errorMessage =
                GetErrorDescription(15)
                    .ValueOr(() => GetDefaultErrorMessage());
        }

        static void Test10()
        {
            var errorMessage =
                GetErrorDescription(15)
                    .ValueOrMaybe(GetErrorDescriptionViaWebService(15))
                    .ValueOr("Unknown error");
        }

        static void Test11()
        {
            var errorMessage =
                GetErrorDescription(15)
                    .ValueOrMaybe(() => GetErrorDescriptionViaWebService(15))
                    .ValueOr("Unknown error");
        }

        static void Test12()
        {
            var logContents =
                GetLogContents(1)
                    .ValueOrThrow("Unable to get log contents");
        }

        static void Test14()
        {
            List<string> multipleLogContents =
                Enumerable.Range(1, 20)
                    .Select(x => GetLogContents(x))
                    .GetItemsWithValue()
                    .ToList();
        }

        static void Test15()
        {
            List<string> multipleLogContents =
                Enumerable.Range(1, 20)
                    .Select(x => GetLogContents(x))
                    .IfAllHaveValues()
                    .ValueOrThrow("Some logs are not available")
                    .ToList();
        }

        static void Test16()
        {
            Maybe<string> logMaybe = Maybe.Some("entry9");

            var list = new List<string>()
            {
                "entry1",
                logMaybe.ToAddIfHasValue(),
                "entry2"
            };
        }

        static string GetDefaultErrorMessage()
        {
            return File.ReadAllText("c:\\defaultErrorMessage.txt");
        }

        static Maybe<string> GetLogContents(int id)
        {
            var filename = "c:\\logs\\" + id + ".log";

            if (File.Exists(filename))
                return File.ReadAllText(filename);

            return Maybe.None;
        }

        static Maybe<int> FindErrorCode(string logContents)
        {
            var logLines = logContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            return
                logLines
                    .FirstOrNone(x => x.StartsWith("Error code: "))
                    .Map(x => x.Substring("Error code: ".Length))
                    .Bind(x => x.TryParseToInt());
        }

        static Maybe<string> GetErrorDescription(int errorCode)
        {
            var filename = "c:\\errorCodes\\" + errorCode + ".txt";

            if (File.Exists(filename))
                return File.ReadAllText(filename);

            return Maybe.None;
        }

        static Maybe<string> GetErrorDescription(int errorCode, string logContents)
        {
            var logLines = logContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            var linePrefix = "Error description for code " + errorCode + ": ";

            return
                logLines
                    .FirstOrNone(x => x.StartsWith(linePrefix))
                    .Map(x => x.Substring(linePrefix.Length));
        }

        static Maybe<string> GetErrorDescriptionViaWebService(int errorCode)
        {
            //Real code would call a web service
            return Maybe.None;
        }
    }
}
