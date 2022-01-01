using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.Tools
{
    public enum STokenType
    {
        Subroutine,
        ParameterListStart,
        ParameterListEnd,
        ParameterListSeperator,
        Number,
        String,
        EOL
    };

    //public class SToken
    //{
    //    STokenType Type;
    //    String Data;
    //};
    public class StringTokenizer
    {
        public static List<String> Tokenise(String s)
        {
            List<String> strings = new List<string>();

            String c = "";
            int i = 0;
            while ( i < s.Length)
            {
                String t = s.Substring(i, 1);
                if (String.IsNullOrWhiteSpace(t))
                {
                    if (c != "")
                    {
                        strings.Add(c);
                        c = "";
                    }
                    i++;
                }
                else
                {
                    switch (t)
                    {
                        case "\t":
                        case ",":
                             if (c != "")
                            {
                                strings.Add(c);
                                c = "";
                            }
                            i++;
                            break;

                        case ")":
                        case ";":
                        case "(":
                       
                            if (c != "")
                            {
                                strings.Add(c);
                                c = "";
                            }
                            i++;
                            strings.Add(t);
                            break;

                        case "\"":
                            {
                                i++;
                                t = s.Substring(i, 1);
                                while (t!="\"")
                                {
                                    c += t;
                                    i++;
                                    t = s.Substring(i, 1);
                                }
                                i++;
                                strings.Add(c);
                                c = "";
                            }
                            break;
                        default:
                            c += t;
                            i++;
                            break;

                    }
                }
            }


            return strings;
        }
    }
}
