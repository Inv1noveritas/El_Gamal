using System;
using System.Numerics;
using System.IO;

namespace El_Gamal
{
    class Program
    {
        static void Main(string[] args)
        {
            BigInteger p, y, g, x, a, b;

            // Запрос сообщения от пользователя
            Console.Write("Введите M: ");
            string m_old = Console.ReadLine();
            BigInteger m = BigInteger.Parse(m_old);

            // Вычисление основных переменных для алгоритма (p, y, g, x)
            generation_keys(out p, out y, out g, out x);

            //ключи
            BigInteger[] secret_key = { x };
            BigInteger[] public_key = { y, g, p };

            Console.WriteLine($"secret_key (x) = {secret_key[0]}");
            Console.WriteLine($"public_key (y, g, p) = {public_key[0]}, {public_key[1]}, {public_key[2]}");

            // Генерация подписи: переменных a и b
            message(secret_key, public_key, m, out a, out b);

            // Проверка подписи
            bool check = check_message(public_key, m, a, b);

            // Если проверка прошла, т.е. y^a * a^b mod p = g^M mod p
            if (check)
            {
                Console.WriteLine("Ваша подпись подошла");
                Console.WriteLine($"a = {a}");
                Console.WriteLine($"b = {b}");
            } else
            {
                Console.WriteLine("Ваша подпись не подошла");
            }
        }

        static void message(BigInteger[] secret_key, BigInteger[] public_key, BigInteger m, out BigInteger a, out BigInteger b)
        {
            BigInteger u, v, k;

            // Выбор случайноко числа k, взаимно простого с p - 1
            k = random_bigInteger_with_diapason(2, public_key[2] - 2);

            BigInteger i = Efklid(k, public_key[2] - 1, out u, out v);

            while (i > 1)
            {
                k = random_bigInteger_with_diapason(2, public_key[2] - 2);
                i = Efklid(k, public_key[2] - 1, out u, out v);
            }

            //a = g ^ k mod p.

            // Первая часть подписи
            a = pow(public_key[1], k, public_key[2]);
            Console.WriteLine($"k = {k}");

            //b = ((M − x · a) · u) mod(p − 1)

            // Вторая часть подписи
            BigInteger b_short = (m - secret_key[0] * a) * u;

            if (b_short < 0)
            {
                b = (b_short % (public_key[2] - 1)) + (public_key[2] - 1);
            } else
            {
                b = b_short % (public_key[2] - 1);
            }

            k = 0;

        }

        static bool check_message(BigInteger[] public_key, BigInteger m, BigInteger a, BigInteger b)
        {
            BigInteger u, v, w, s;
            //BigInteger[] public_key = { y, g, p };
            //u = ModExp(y, a, p)
            u = pow(public_key[0], a, public_key[2]);
            //v = ModExp(a, b, p)
            v = pow(a, b, public_key[2]);
            //w = (u · v) mod p
            w = (u * v) % public_key[2];
            //s = ModExp(g, M, p)
            s = pow(public_key[1], m, public_key[2]);
            //if w = s then
            //  return true
            //return false

            Console.WriteLine($"w = {w}");
            Console.WriteLine($"s = {s}");

            if (w == s)
            {
                return true;
            }
            return false;

        }

        static void generation_keys(out BigInteger p, out BigInteger y, out BigInteger g, out BigInteger x)
        {
            p = random_bigInteger();

            while (!function_Rabin_Miller(p, 5))
            {
                p = random_bigInteger();
            }

            x = random_bigInteger_with_diapason(2, p - 2);
            g = random_bigInteger_with_diapason(2, p - 2);

            y = pow(g, x, p);

        }

        static BigInteger Efklid(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            if (b < a)
            {
                var t = a;
                a = b;
                b = t;
            }

            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }

            BigInteger gcd = Efklid(b % a, a, out x, out y);

            BigInteger newY = x;
            BigInteger newX = y - (b / a) * x;

            x = newX;
            y = newY;

            return gcd;
        }

        //генерация рандомного BigInteger
        static BigInteger random_bigInteger()
        {
            string number = "";
            Random random_numbers = new Random();
            //
            // !!
            // вот тут менять цифры, если нужна другая длина ключа
            // !!
            //
            int end = random_numbers.Next(32, 50);

            for (int i = 0; i < end; i++)
            {
                int num = random_numbers.Next(0, 9);
                number += num.ToString();
            }

            BigInteger posBigInt = BigInteger.Parse(number);

            return posBigInt;
        }

        //проверка на четность
        static bool checking_for_odd(BigInteger x)
        {
            if (x % 2 == 0)
                return false;
            return true;
        }

        // тест Миллера — Рабина на простоту числа
        // производится k раундов проверки числа n на простоту
        static bool function_Rabin_Miller(BigInteger n, int k)
        {
            if (n == 2 || n == 3)
                return true;

            if (n < 3 || !checking_for_odd(n))
                return false;

            // представим n − 1 в виде (2^s)·t, где t нечётно, это можно сделать последовательным делением n - 1 на 2
            BigInteger t = n - 1;

            int s = 0;

            while (t % 2 == 0)
            {
                t /= 2;
                s += 1;
            }

            // повторить k раз
            for (int i = 0; i < k; i++)
            {
                //Выбрать случайное целое число a в отрезке [2, n − 2]
                BigInteger a = random_bigInteger_with_diapason(2, n - 2);

                //x ← a^t mod n, вычисляется с помощью алгоритма возведения в степень по модулю
                BigInteger x = pow(a, t, n);

                //x = 1 или x = n − 1, то перейти на следующую итерацию цикла i
                if (x == 1 || x == n - 1)
                {
                    continue;
                }

                //повторить s − 1 раз
                for (int j = 0; j < s - 1; j++)
                {
                    //x ← x^2 mod n
                    x = pow(x, 2, n);

                    if (x == 1)
                        return false; //составное

                    //перейти на следующую итерацию цикла i
                    if (x == n - 1)
                        break;
                }

                //личная проверка личного производства
                if (no_check_simple(n))
                    return false;

                if (x != n - 1)
                    return false;

            }
            return true; //вероятно простое
        }

        //генерация рандомного BigInteger в диапазоне от a до b
        static BigInteger random_bigInteger_with_diapason(BigInteger a, BigInteger b)
        {
            string number = "";
            Random random_numbers = new Random();
            //
            // !!
            // вот тут менять цифры, если нужна другая длина ключа
            // !!
            //
            int end = random_numbers.Next(1, 32);
            BigInteger posBigInt = 0;

            //пока число не находится в необходимом диапазоне, будет генерироваться рандомное число
            while (posBigInt <= a || posBigInt >= b)
            {
                number = "";

                for (int i = 0; i < end; i++)
                {
                    int num = random_numbers.Next(0, 9);
                    number += num.ToString();
                }

                posBigInt = BigInteger.Parse(number);

            }

            return posBigInt;
        }

        //проверка на составное число: дополнительно
        static bool no_check_simple(BigInteger x)
        {
            //массив простых чисел до 1000

            StreamReader sr = new StreamReader("check.txt");
            string line;
            string lines = null;

            line = sr.ReadLine();
            while (line != null)
            {
                lines += line;
                //Read the next line
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();

            char[] charsToTrim = { ' ' };
            string result = lines.Trim(charsToTrim);

            string[] subs = result.Split(',');
            int[] prow = new int[168];
            int i = 0;

            foreach (string s in subs)
            {
                prow[i] = int.Parse(s);
                i++;
            }

            //если число делится на кого-то из массива и не является каким-то числом из этого массива
            foreach (int qwe in prow)
            {
                if (x % qwe == 0 && x != qwe)
                    return true;
            }

            return false; //возможно простое
        }

        //возведение в степень по модулю
        static BigInteger pow(BigInteger x, BigInteger y, BigInteger n)
        {
            if (y == 0)
            {
                return 1;
            }

            if (x == 0)
            {
                return 0;
            }

            BigInteger z = pow(x, y / 2, n);

            if (y % 2 == 0)
                return (z * z) % n;
            else
                return (x * z * z) % n;
        }
    }
}
