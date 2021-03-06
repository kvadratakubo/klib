﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace klib
{
    public class Dynamic
    {
        public readonly dynamic Value;
        public int Lenght => GetLenght();
        public static Dynamic Empty => new Dynamic("");
        public string Description => Name;
        public string Name { get; protected set; }
        /// <summary>
        /// Transport more informations 
        /// </summary>
        public string Comments = String.Empty;

        public Dynamic(dynamic value)
        {
            Value = value;
        }

        public Dynamic(dynamic value, string name)
        {
            Value = value;
            Name = name;
        }

        #region VAL: Validations
        public bool ValStartWith(string val)
        {
            return Value.ToString().StartsWith(val, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Return Email valid
        /// </summary>
        /// <param name="mail_alt">If invalid e-mailo, return alternative e-mail</param>
        /// <returns>Email</returns>
        public string ValEmail(string mail_alt = null)
        {
            Regex regex = new Regex(E.RegexMask.Email);
            Match match = regex.Match(Value.ToString());
            if (match.Success)
                return ToString();
            else
            {
                if (!string.IsNullOrEmpty(mail_alt) && regex.Match(mail_alt).Success)
                    return mail_alt;
                else
                    throw new KLIBException(7, ToString(), "e-mail");
            }
        }

        public bool ValTrue => ToBool();
        public bool ValFalse => !ToBool();
        #endregion
        #region INF: Information
        public string InfTypeOf => Value.GetType().Name.ToUpper();
        #endregion
        
        #region IS: Question
        /// <summary>
        /// Verify the value is type of numeric
        /// </summary>
        /// <param name="tryParse">The method will ignore the type and attempt to parse</param>
        /// <returns></returns>
        public bool IsNumber(bool tryParse = false)
        {
            //0002
            if (tryParse)
            {
                try
                {
                    var foo = Decimal.Parse(Value.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                switch (InfTypeOf)
                {
                    case "DOUBLE":
                    case "INT":
                    case "INT16":
                    case "INT32":
                    case "INT64":
                    case "DECIMAL":
                        return true;
                    default: return false;
                }
            }
        }

        public bool IsEmpty => Value == null
            || String.IsNullOrEmpty(Value.ToString())
            || String.IsNullOrWhiteSpace(Value.ToString());

        #endregion

        #region Convert
        public bool ToBool(string totrue = null)
        {
            //00002         
            var foo = Value.ToString().ToUpper();
            if (foo != null && totrue != null && foo == totrue.ToUpper())
                return true;

            switch (foo)
            {
                case "1":
                case "Y":
                case "YES":
                case "S":
                case "SI":
                case "SIM":
                case "T":
                case "TRUE":
                    return true;
                default:
                    return false;

            }
        }

        public char ToCharBool(char yes = 'Y', char no = 'N')
        {
            return ToBool(yes.ToString()) ? yes : no;
        }
        public string ToStringBool(string yes = "YES", string no = "NO")
        {
            return ToBool(yes.ToString()) ? yes : no;
        }
        public decimal ToDecimal()
        {
            try
            {
                return Decimal.Parse(Value.ToString());
            }catch(Exception ex)
            {
                throw new KLIBException(5, Value.ToString(), "Decimal", ex.Message);
            }
        }

        #endregion
        #region Objects
        public Uri ToUrl()
        {
            try
            {
                return new Uri(Value.ToString());
            }
            catch (System.UriFormatException ex)
            {
                throw new KLIBException(5, Value.ToString(), ex.Message);
            }
        }
        public FileInfo ToFile()
        {
            return new FileInfo(ToString());
        }
        #endregion

        /// ///////////////////////////////////////////////////////////////////////////////////
        public DirectoryInfo ToDirectory()
        {
            if (System.IO.Directory.Exists(Value.ToString()))
                return new DirectoryInfo(Value.ToString());

            switch(Value.ToString().ToUpper())
            {
                case "%TEMP%": return Shell.TempDir();
                default: throw new KLIBException(1, $"Directory not exists or without permissions. {Value.ToString()}");
            }
        }


        private int GetLenght()
        {
            if(Value.GetType() == typeof(String))
                return Value.ToString().Length;
            if (Value.GetType() == typeof(Array))
                return ((Array)Value).Length;
            else
                throw new KLIBException(1,$"({Value.GetType()}) Type of variable isn't not defined.");
        }


        public byte[] ToByteArray()
        {
            int NumberChars = Value.ToString().Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(Value.ToString().Substring(i, 2), 16);
            return bytes;
        }

        public object ToObject()
        {
            return Value;
        }

        public int ToInt(int ifnull = 0)
        {
            try
            {
                return int.Parse(Value.ToString());
            }
            catch
            {
                return ifnull;
            }
        }

        public int? ToNInt()
        {
            try
            {
                return int.Parse(Value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public string ToJson()
        {
            throw new NotImplementedException();
            //return Newtonsoft.Json.JsonConvert.SerializeObject(Value);
        }

        public dynamic ToAny()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }


        public DateTime ToDateTime(string format)
        {
            return DateTime.ParseExact(Value.ToString(),format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public int TimeToInt(bool addsecs = false)
        {
            if (addsecs)
                return int.Parse(ToDateTime().ToString("HHmmss"));
            else
                return int.Parse(ToDateTime().ToString("HHmm"));
        }

        public DateTime ToDateTime()
        {
            return (DateTime)Value;
        }

        public DateTime ToDate()
        {
            return ToDateTime().Date;
        }
        public double ToDouble(string point)
        {
            var val = Value.ToString();

            if (point == ".")
                val = val.Replace(",", "");
            else
                val = val.Replace(".", "");

            val = val.Replace(",", ".");
            return double.Parse(System.Text.RegularExpressions.Regex.Replace(Value.ToString(), "[^0-9.]+", ""));
        }

        public Dynamic OnlyNumbers(int def = 0)
        {
            if (IsEmpty)
                return new Dynamic(def);

            var val = Regex.Match(ToString(), @"\d+").Value;

            if (String.IsNullOrEmpty(val))
                return new Dynamic(def);
            else
                return new Dynamic(val);

        }

        /// <summary>
        /// Replace information using regex. Exemple:
        /// Only char and number: "[^0-9a-zA-Z]+"
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string RegexReplace(string pattern)
        {
            return Regex.Replace(Value.ToString(), pattern, "");
        }

        public string ToStringFormat(params object[] values)
        {
            if (values.Length > 0)
                return String.Format(Value.ToString(), values);
            else
                return Value.ToString();
        }

        public byte[] ToByte()
        {
            if (Value.GetType().Name == "Byte[]")
                return (byte[])Value;
            else
                throw new KLIBException(1, "Value isn't byte[]");
        }

        #region DateTime
        /// <summary>
        /// Compare the dates
        /// </summary>
        /// <param name="dateTime">Date to compare (exclude the seconds)</param>     
        /// <returns>
        /// 1 - Value is great then dateTime
        /// 0 - Value and dateTime same values
        /// -1 - dateTime is great then Value
        /// </returns>
        public int Compare(DateTime dateTime)
        {
            var valDateTime = (DateTime)Value;
            var date1 = new DateTime(valDateTime.Year, valDateTime.Month, valDateTime.Day, valDateTime.Hour, valDateTime.Minute, 0);
            var date2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);                        

            return DateTime.Compare(date1, date2);
        }
        #endregion

        #region IO
        public model.Printer Printer(bool validate = true)
        {
            var printer = new model.Printer(Value.ToString());

            if (validate && !printer.ValidPrinter())
                throw new KLIBException(1, $"Printer isn't {Value.ToString()} valid");

            return printer;
        }

        //public System.IO.DirectoryInfo Directory(bool validate = true)
        //{
        //    if (validate && !System.IO.Directory.Exists(Value.ToString()))
        //        throw new LException(1, $"Directory {Value.ToString()} not exists");

        //    return new System.IO.DirectoryInfo(Value.ToString());
        //}
        #endregion


    }


    public class DicDymanic //: IDictionary<string, Dynamic>, ICollection<KeyValuePair<string, Dynamic>>, IEnumerable<KeyValuePair<string, Dynamic>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<string, Dynamic>, IReadOnlyCollection<KeyValuePair<string, Dynamic>>, ISerializable, IDeserializationCallback
    {
       
        private Dictionary<string, Dynamic> Dic = new Dictionary<string, Dynamic>();
        public int Count => Dic.Count;

        public Dynamic this[string key]
        { get
            {
                return Dic[key];
            }
            set
            {
                Dic[key] = value;
            }
        }

                
        public void Add(Dynamic dyn)
        {
            this[dyn.Name] = dyn;   
        }
    }


    public static class ValuesEx
    {
        
        public static Dynamic To(object value)
        {
            return new Dynamic(value);
        }     
        public static string RegexReplace(object val, string pattern)
        {
            return To(val).RegexReplace(pattern);
        }

        /// <summary>
        /// Validate the value using regex
        /// </summary>
        /// <param name="val">Value to validate</param>
        /// <param name="pattern">Regex format (klib.E.RegexMask)</param>
        /// <returns></returns>
        public static bool RegexValidate(object val, string pattern)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(val.ToString());
            return match.Success;
        }

        /// <summary>
        /// Format values in characteres open and close.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="open"></param>
        /// /// <param name="close"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Format(string val, string open, string close, params object[] values)
        {
            // Query manager = @"\[\%[0-9]*\]"
            var pattern = $@"{open}[0-9]*{close}";
            int qtty = Regex.Matches(val, pattern).Count;

            for(int i = 0; i < qtty; i++)
                val = val.Replace($@"{open}{i}{close}", values[i].ToString());

            return val;
        }

        public static StreamReader ResourceUrl(Assembly assembly, Type classType, E.Resource dir)
        {
            return ResourceUrl(assembly, classType.Name, dir);
        }
        public static StreamReader ResourceUrl(Assembly  assembly, string name, E.Resource dir)
        {
            var resource = assembly.GetManifestResourceNames();
            var resourceName = resource.Where(t => t.Contains($"content.{dir.ToString()}.{name}")).FirstOrDefault();
            if (!String.IsNullOrEmpty(resourceName))
                return new StreamReader(assembly.GetManifestResourceStream(resourceName));
            else
                return null;
        }
    }
}
