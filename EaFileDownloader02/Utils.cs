using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace EaFileDownloader02
{
    static class Utils
    {
        /// <summary>
        /// Валидатор E-mail
        /// W3C https://html.spec.whatwg.org/multipage/input.html#e-mail-state-(type=email)
        /// </summary>
        /// <remarks>
        /// Различие с оригинальным рег. выражением - обязательное наличие домена верхнего уровня
        /// </remarks>
        public static readonly Regex EmailRegex
            = new Regex(@"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// Проверить ИНН
        /// </summary>
        /// <param name="inn">Значение ИНН</param>
        /// <param name="juralPerson">Признак, юр. или физ. лицо</param>
        /// <returns>Результат проверки</returns>
        //public static bool VerifyInn(string inn, bool juralPerson)
        //{
        //    if (string.IsNullOrEmpty(inn))
        //    {
        //        return false;
        //    }

        //    if ((juralPerson && inn.Length != 10) || (!juralPerson && inn.Length != 12))
        //    {
        //        return false;
        //    }

        //    if (!Utils.IsNumber(inn))
        //    {
        //        return false;
        //    }

        //    var success = false;
        //    if (juralPerson)
        //    {
        //        int digit1 = inn[0].ToInt() * 2;
        //        int digit2 = inn[1].ToInt() * 4;
        //        int digit3 = inn[2].ToInt() * 10;
        //        int digit4 = inn[3].ToInt() * 3;
        //        int digit5 = inn[4].ToInt() * 5;
        //        int digit6 = inn[5].ToInt() * 9;
        //        int digit7 = inn[6].ToInt() * 4;
        //        int digit8 = inn[7].ToInt() * 6;
        //        int digit9 = inn[8].ToInt() * 8;
        //        int digit10 = inn[9].ToInt();

        //        int sum = digit1 + digit2 + digit3 + digit4 + digit5 +
        //            digit6 + digit7 + digit8 + digit9;

        //        int mod = sum % 11;
        //        if (mod == 10)
        //        {
        //            mod = 0;
        //        }

        //        if (mod == digit10)
        //        {
        //            success = true;
        //        }
        //    }
        //    else
        //    {
        //        int digit1 = inn[0].ToInt() * 7;
        //        int digit2 = inn[1].ToInt() * 2;
        //        int digit3 = inn[2].ToInt() * 4;
        //        int digit4 = inn[3].ToInt() * 10;
        //        int digit5 = inn[4].ToInt() * 3;
        //        int digit6 = inn[5].ToInt() * 5;
        //        int digit7 = inn[6].ToInt() * 9;
        //        int digit8 = inn[7].ToInt() * 4;
        //        int digit9 = inn[8].ToInt() * 6;
        //        int digit10 = inn[9].ToInt() * 8;
        //        int digit11 = inn[10].ToInt();

        //        int sum = digit1 + digit2 + digit3 + digit4 + digit5 +
        //            digit6 + digit7 + digit8 + digit9 + digit10;

        //        int mod = sum % 11;
        //        if (mod == 10)
        //        {
        //            mod = 0;
        //        }

        //        if (mod == digit11)
        //        {
        //            digit1 = inn[0].ToInt() * 3;
        //            digit2 = inn[1].ToInt() * 7;
        //            digit3 = inn[2].ToInt() * 2;
        //            digit4 = inn[3].ToInt() * 4;
        //            digit5 = inn[4].ToInt() * 10;
        //            digit6 = inn[5].ToInt() * 3;
        //            digit7 = inn[6].ToInt() * 5;
        //            digit8 = inn[7].ToInt() * 9;
        //            digit9 = inn[8].ToInt() * 4;
        //            digit10 = inn[9].ToInt() * 6;
        //            digit11 = inn[10].ToInt() * 8;
        //            int digit12 = inn[11].ToInt();

        //            sum = digit1 + digit2 + digit3 + digit4 + digit5 +
        //                digit6 + digit7 + digit8 + digit9 + digit10 + digit11;

        //            mod = sum % 11;
        //            if (mod == 10)
        //            {
        //                mod = 0;
        //            }

        //            if (mod == digit12)
        //            {
        //                success = true;
        //            }
        //        }
        //    }

        //    return success;
        //}

        /// <summary>
        /// Проверить огрн
        /// </summary>
        /// <param name="ogrn">Значение огрн</param>
        /// <param name="juralPerson">Признак, юр. или физ. лицо</param>
        /// <returns>Результат проверки</returns>
        public static bool VerifyOgrn(string ogrn, bool juralPerson)
        {
            if (string.IsNullOrEmpty(ogrn))
            {
                return false;
            }

            if ((juralPerson && ogrn.Length != 13) || (!juralPerson && ogrn.Length != 15))
            {
                return false;
            }

            long значение;
            if (!long.TryParse(ogrn, out значение))
            {
                return false;
            }

            var успех = false;

            if (juralPerson)
            {
                long числоБезПоследнегоЗнака;
                if (!long.TryParse(ogrn.Substring(0, 12), out числоБезПоследнегоЗнака))
                {
                    return false;
                }

                long последнийЗнакЧисла;
                if (!long.TryParse(ogrn.Substring(12, 1), out последнийЗнакЧисла))
                {
                    return false;
                }

                long числоПослеДеления = числоБезПоследнегоЗнака / 11;
                long числоПослеУмножения = числоПослеДеления * 11;
                long числоПослеВычитания = числоБезПоследнегоЗнака - числоПослеУмножения;

                числоПослеВычитания = числоПослеВычитания > 9 ? числоПослеВычитания % 10 : числоПослеВычитания;

                if (последнийЗнакЧисла == числоПослеВычитания)
                {
                    успех = true;
                }
            }
            else
            {
                long числоБезПоследнегоЗнака;
                if (!long.TryParse(ogrn.Substring(0, 14), out числоБезПоследнегоЗнака))
                {
                    return false;
                }

                long последнийЗнакЧисла;
                if (!long.TryParse(ogrn.Substring(14, 1), out последнийЗнакЧисла))
                {
                    return false;
                }

                long числоПослеДеления = числоБезПоследнегоЗнака / 13;
                long числоПослеУмножения = числоПослеДеления * 13;
                long числоПослеВычитания = числоБезПоследнегоЗнака - числоПослеУмножения;

                числоПослеВычитания = числоПослеВычитания > 9 ? числоПослеВычитания % 10 : числоПослеВычитания;

                if (последнийЗнакЧисла == числоПослеВычитания)
                {
                    успех = true;
                }
            }

            return успех;
        }

        /// <summary>
        /// Проверить штрихкод
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns>Результат проверки</returns>
        public static bool VerifyBarcode(string barcode)
        {
            int cnt = 0;
            int sum = 0;

            if (string.IsNullOrEmpty(barcode))
            {
                return false;
            }

            if (barcode.Length != 13)
            {
                return false;
            }

            for (int j = barcode.Length - 2; j > -1; j--)
            {
                cnt += 1;

                if (cnt % 2 == 0)
                {
                    sum += int.Parse(barcode[j].ToString());
                }
                else
                {
                    sum += int.Parse(barcode[j].ToString()) * 3;
                }
            }

            sum = (10 - (sum % 10)) % 10;

            if (barcode.Substring(barcode.Length - 1, 1) == sum.ToString())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Метод получения Min для > 2 переменных
        /// </summary>
        public static T Min<T>(params T[] vals)
        {
            return vals.Min();
        }

        /// <summary>
        /// Метод получения Max для > 2 переменных
        /// </summary>
        public static T Max<T>(params T[] vals)
        {
            return vals.Max();
        }

        /// <summary>
        /// Регулярное выражение содержащее недоступные символы в имени файла <see cref="Path.GetInvalidFileNameChars"/>
        /// </summary>
        public static readonly Regex InvalidFileNameCharsRegex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        /// <summary>
        /// Заменяем недопустимые символы ОС в имени файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="replaceString">Строка замены, каждый недопустимый символ будет заменён на это значение</param>
        public static string RepairPathFileName(string fileName, string replaceString = "_")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            return Utils.InvalidFileNameCharsRegex.Replace(fileName, replaceString);
        }




        private static readonly Expression<Func<decimal?, decimal?>> CalcNdsTree = x => x * 18 / 118;
        private static readonly Expression<Func<decimal?, decimal?, decimal?>> CalcVariableNdsTree = (x, nds) => x * nds / (100 + nds);
        private static Func<decimal?, decimal?> CalcNdsCompiled;
        private static Func<decimal?, decimal?, decimal?> CalcVariableNdsCompiled;

        private static bool IsNumber(string str)
        {
            var regex = new Regex(@"^\d+$");
            return regex.IsMatch(str);
        }

        /// <summary>
        /// Получить наименование метода по стеку вызовов
        /// </summary>
        /// <param name="deep">Глубина стека</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetMethodName(int deep = 1)
        {
            var stackFrame = new StackTrace().GetFrame(deep);

            return stackFrame.GetMethod().Name;
        }

        /// <summary>
        /// Расчет процента
        /// </summary>
        /// <param name="count">Количество</param>
        /// <param name="totalCount">Общее количество</param>
        /// <param name="decimals">Число знаков после запятой</param>
        /// <returns></returns>
        public static decimal CalcPercent(decimal count, decimal totalCount, int decimals = 0)
        {
            return totalCount != 0
                ? Math.Round(count / totalCount * 100, decimals, MidpointRounding.AwayFromZero)
                : 0;
        }

        public static bool IsEmail(string email)
        {
            return Utils.EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// Возвращает false, если все false или все true
        /// <para>Строит выражение вида (value | otherValue1 | ... | otherValueN) ^ (value & otherValue1 & ... & otherValueN)</para>
        /// </summary>
        public static bool OrXorAnd(this bool value, params bool[] otherValues)
        {
            if (otherValues == null)
            {
                return value;
            }

            var lValue = value;
            var rValue = value;
            foreach (var otherValue in otherValues)
            {
                lValue |= otherValue;
                rValue &= otherValue;
            }

            return lValue ^ rValue;
        }

        /// <summary>
        /// Комбинирует путь
        /// </summary>
        public static string CombinePath(params string[] pathParts)
        {
            if (pathParts.Length == 0)
            {
                return null;
            }

            var result = new StringBuilder();
            foreach (var part in pathParts)
            {
                if (result.Length > 0)
                {
                    result.Append('/');
                }
                result.Append(part.Trim('/'));
            }

            return result.ToString();
        }
    }
}
