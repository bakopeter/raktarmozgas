using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Xml.Linq;

namespace raktarmozgas
{
    internal class Program
    {
        /*Beolvassa és eltárolja a fájl tartalmát, ellenőrzi, hogy a fájlban vannak-e hibák, illetve üres sorok, amiről üzenetet is küld.*/
        static void LoadFile(string path, string output)
        {
            StreamReader sr = new(path);

            string header = sr.ReadLine();

            if (output == "display")
            {
                header = header.Replace(';', '\t');
                Console.WriteLine($"\t{header}");
            }

            while (!sr.EndOfStream)
            {
                string row = "";
                try
                {
                    row = sr.ReadLine();
                    switch (output)
                    {
                        case "keszletMozgas":
                            KeszletMozgas.mozgasok.Add(KeszletMozgas.CreateKeszletMozgas(row));
                            break;
                        case "display":
                            row = row.Replace(';', '\t');
                            Console.WriteLine($"\t{row}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Figyelem: Hibás sor! - \"{row}\" ({e.Message})");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            sr.Close();
        }

        /*Kiírja a legnagyobb értékben és mennyiségben szállító partner nevét. (A komment zárójelek eltávolításával, és a Partner struktura 
         * konstruktorának kikommentelésével itt is meg lehet valósítani a max/min kiválasztás műveletét.)*/
        static void DisplayMaxTransport(List<Partner> partnerek, Partner Maxmenny, Partner Maxertek)
        {
            //Partner.partnerek = Partner.CreatePartnerList(mozgas);

            //Partner maxM = Partner.partnerek.MaxBy(m => m.mennyiseg);
            //var maxE = Partner.partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség:\t{Maxmenny.Nev}\t{Maxmenny.Mennyiseg} kg\t{Maxmenny.Ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték:\t{Maxertek.Nev}\t{Maxertek.Mennyiseg} kg\t{Maxertek.Ertek} Ft.");
        }

        /*Kiírja az össz napi forgalom mennyiségét és értékét.*/
        static void DisplayTradeFlow(float[] osszmenny, double[] osszertek)
        {
            Console.WriteLine($"\tBeszállított termékek\tmennyisége: {osszmenny[(int)MozgasTipus.BESZERZES]} " +
                $"kg\tösszértéke: {Math.Round(osszertek[(int)MozgasTipus.BESZERZES])} Ft.");

            Console.WriteLine($"\tEladott termékek\tmennyisége: {osszmenny[(int)MozgasTipus.ELADAS]} " +
                $"kg\tösszértéke: {Math.Round(osszertek[(int)MozgasTipus.ELADAS])} Ft.");
        }

        /*Kilistázza azon termékek mennyiségeit, melyek készlete 50% alá esett.*/
        static void DisplayProductsToOrder(string[] kifogyoTermekek, float[,] mennyisegek)
        {
            int k = 0;

            Console.WriteLine("\n\tTermék\t\tRend.\tEladás\tKészlet");

            while (k < kifogyoTermekek.Length && kifogyoTermekek[k]is not null)
            {
                float beszerzes = mennyisegek[k, (int)MozgasTipus.BESZERZES];
                float eladas = mennyisegek[k, (int)MozgasTipus.ELADAS];
                float keszlet = beszerzes - eladas;

                Console.WriteLine($"\t{kifogyoTermekek[k]}\t{beszerzes}\t{eladas}\t{keszlet} kg.");
                k++;
            }
        }

        /*Kiírja fájlba a rendelési listát*/
        static void PrintOrderList(List<Termek> rendeles, string path)
        {
            StreamWriter sr = new(path);

            sr.WriteLine("az;ido;rend_termek;menny;partner");

            foreach (var item in rendeles)
            {
                sr.WriteLine(Termek.CollectProductDetails(item));
            }
            sr.Close();
            
        }

        /*Termékenként összegzi a napi bevételt és kiadást, majd ebből kiszámolja és kiírja a profitot.*/
        static void DailyProfit(string[] termekek, float[,] mennyisegek, double[,] termekArak)
        {
            double osszKiadas = 0;
            double osszBevetel = 0;
            double osszProfit = 0;

            Console.WriteLine("\n\tTermékeladás\tBevétel\tKiadás\tHaszon");

            int k = 0;
            while (k < termekek.Length && termekek[k] is not null)
            {
                double kiadas = Math.Round(mennyisegek[k, (int)MozgasTipus.ELADAS] * termekArak[k, (int)MozgasTipus.BESZERZES]);
                osszKiadas += kiadas;
                double bevetel = Math.Round(mennyisegek[k, (int)MozgasTipus.ELADAS] * termekArak[k, (int)MozgasTipus.ELADAS]);
                osszBevetel += bevetel;
                double profit = bevetel - kiadas;
                osszProfit = osszBevetel - osszKiadas;

                Console.WriteLine($"\t{termekek[k]}\t{bevetel}\t{kiadas}\t{profit} Ft");
                k++;
            }

            Console.WriteLine($"\tÖsszesen\t{osszBevetel}\t{osszKiadas}\t{osszProfit} Ft");
        }

        /*Kiszámolja és kiírja melyik órában volt a legnagyobb forgalom, forgalom típusa szerint is.*/
        static void MaxTradeFlow(int[,] forgalmak, int nyitas, int zaras) 
        {
            int[] forgalom = new int[24];

            for (int i = 0; i < forgalmak.GetLength(0); i++)
            {
                for (int j = 0; j < forgalmak.GetLength(1); j++) 
                    forgalom[i] += forgalmak[i, j];
            }

            int maxForg = forgalom.Max();
            int iOfMaxF = Array.IndexOf(forgalom, forgalom.Max());
            int maxBesz = forgalmak[nyitas, (int)MozgasTipus.BESZERZES];
            int iOfMaxB = nyitas;
            int maxElad = forgalmak[nyitas, (int)MozgasTipus.ELADAS];
            int iOfMaxE = nyitas;

            for (var i = nyitas + 1; i <= zaras; i++)
            {
                if (forgalmak[i, (int)MozgasTipus.BESZERZES] > maxBesz)
                {
                    iOfMaxB = i;
                    maxBesz = forgalmak[iOfMaxB, (int)MozgasTipus.BESZERZES];
                }
                else if (forgalmak[i, (int)MozgasTipus.ELADAS] > maxElad)
                {
                    iOfMaxE = i;
                    maxElad = forgalmak[iOfMaxE, (int)MozgasTipus.ELADAS];
                }
            }

            Console.WriteLine($"\t{iOfMaxF} óra:\t{maxForg} db. forgalom (beszerzés/eladás)");
            Console.WriteLine($"\t{iOfMaxB} óra:\t{maxBesz} db. beszerzés");
            Console.WriteLine($"\t{iOfMaxE} óra:\t{maxElad} db. eladás");
        }

        /*A felhasználótól bekért időpontok szerint kiszámolja és kiírja, hogy az adott órákban mennyi beszerzés és eladás volt. Addig 
         kérhetjük az időpontokat, amíg egy "üres" Entert, vagy nem numerikus billentyűt és Entert nem nyomunk. Ha 24 óránál nagyobb 
        időpontot írunk be, rendszerfigyelmeztetést kapunk, ami után folytathatjuk az órák szerinti lekérdezéseket.*/
        static void HourlySales(int[,] forgalmak)
        {
            bool success = false;
            int ora = 0;
            do
            {
                try
                {
                    Console.Write("\t");
                    success = int.TryParse(Console.ReadLine(), out ora);
                    if (success)
                    {
                        Console.CursorTop -= 1;
                        Console.WriteLine($"\t{ora} órakor {forgalmak[ora, (int)MozgasTipus.BESZERZES]} db. beszerzés és " +
                            $"{forgalmak[ora, (int)MozgasTipus.ELADAS]} db. eladás történt");
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Az óra csak 0-24-ig adható meg - \"{ora}\" óra nem megfelelő ({e.Message})");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            while (success);
        }
        static void Main(string[] args)
        {
            Console.Title = "Hentesüzlet napi készletmozgásai";
            LoadFile("raktarstat.log", "keszletMozgas");

            Console.WriteLine("\nLegtöbbet, és legnagyobb értékben szállító partner");
            DisplayMaxTransport(Partner.CreatePartnerList(KeszletMozgas.mozgasok), Partner.Maxmenny, Partner.Maxertek);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");
            DisplayTradeFlow(Partner.SumTradeFlow(Partner.partnerek), Partner.SumCashFlow(Partner.partnerek));

            Console.WriteLine("\nA készlet 50%-a alá eső termékek, melyek automatikusan hozzáadódnak a rendelési listához");
            DisplayProductsToOrder(KeszletMozgas.OutOfStock(KeszletMozgas.mozgasok), KeszletMozgas.GetProductAmounts(KeszletMozgas.mozgasok));

            Console.WriteLine("\nFájlba kiírt és onnan visszaolvasott rendelési lista ellenőrzése");
            PrintOrderList(Termek.CreateOrderList(KeszletMozgas.mozgasok), "rendeles.txt");
            LoadFile("rendeles.txt", "display");

            Console.WriteLine("\nA napi termékeladásból származó üzleti haszon (eladott mennyiség x (eladási ár - beszerzési ár))");
            DailyProfit(KeszletMozgas.GetProducts(KeszletMozgas.mozgasok), KeszletMozgas.GetProductAmounts(KeszletMozgas.mozgasok), 
                KeszletMozgas.GetProductPrices(KeszletMozgas.mozgasok));

            Console.WriteLine($"\nNapi legforgalmasabb időszakok órák szerint (Lehetnek a megjelenítettel azonos forgalmú időszakok!)");
            MaxTradeFlow(KeszletMozgas.SalesPerHour(KeszletMozgas.mozgasok), 8, 16);

            Console.WriteLine("\nAdja meg, hogy mely órák forgalmi adatait szeretné lekérdezni! (Kilépés: Enter)");
            HourlySales(KeszletMozgas.SalesPerHour(KeszletMozgas.mozgasok));
        }
    }
}