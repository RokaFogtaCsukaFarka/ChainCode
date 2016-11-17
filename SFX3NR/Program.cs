using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SFX3NR
{
    class Program
    {
        public static Int32 sorszam = 0;
        public static Boolean vege = false;

        public static Boolean programFuttatas()
        {
            Console.WriteLine("Milyen sorszámú fájlt szeretnénk feldolgozni?");

            var fajlSzam = Console.ReadLine();
            Regex rgx = new Regex("[0-9]+", RegexOptions.None);
            MatchCollection matches = rgx.Matches(fajlSzam);
            
            if (matches.Count > 0)
                sorszam = Int32.Parse(fajlSzam);

            Logika logika = new Logika();

            int beolvasas = logika.fajlBeolvasas(sorszam);
            switch (beolvasas)
            {
                case 0: break;
                case 1:
                    Console.WriteLine("A bemeneti fájl nem létezik! Kérlek hozd létre!\n");
                    return false;
                case 2:
                    Console.WriteLine("A bemeneti fájl üres. Kérlek javítsd ki!\n");
                    return false;
                case 3:
                    Console.WriteLine("A bemeneti fájl dimenziói hibásak. Kérlek javítsd ki!\n");
                    return false;
                case 4:
                    Console.WriteLine("A bemeneti fájlban nem egy fekete folt van. Kérlek javítsd ki!\n");
                    return false;
                case 5:
                    Console.WriteLine("A bemeneti fájlban a fekete folt közepén egy fehér folt található. Ez hiba! Kérlek javítsd ki!\n");
                    return false;
            }

            logika.fajlKiiras(sorszam);
            Console.WriteLine("Feldolgozás vége.\n");

            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Üdvözlünk a programban!\n");

            programFuttatas();

            while(!vege)
            {
                Console.WriteLine("Szeretnél újabb fájlt feldolgozni? (I/N) ");
                String valasz = Console.ReadLine();

                if (valasz.Equals("I"))
                {                    
                    programFuttatas();
                }
                else if (valasz.Equals("N"))
                {
                    Console.WriteLine("\nA viszontlátásra!");
                    Console.ReadKey();
                    vege = true;
                }
                else
                {
                    Console.WriteLine("Rossz válasz! (I/N)\n");
                }
            }
        }
    }
}
