using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Xml.Linq;

namespace raktarmozgas
{
    /*A főprogramot tartalmazó osztály. Kommunikál a felhasználóval, adatot kér be, meghívja a szükséges információkat megjelenítő 
     * metódusokat, elvégzi a fájlba történő kiiratást.*/
    internal class Program : Display 
    {
        static void Main(string[] args)
        {
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

            Console.Title = "Hentesüzlet napi készletmozgásai";
            LoadFile("raktarstat.log", "keszletMozgas");

            Console.WriteLine("\nLegtöbbet, és legnagyobb értékben szállító partner");
            Partner.DisplayMaxTransport(Partner.CreatePartnerList(KeszletMozgas.mozgasok), Partner.Maxmenny, Partner.Maxertek);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");
            DisplayTradeFlow(Partner.SumTradeFlow(Partner.partnerek), Partner.SumCashFlow(Partner.partnerek));

            Console.WriteLine("\nA készlet 50%-a alá eső termékek, melyek automatikusan hozzáadódnak a rendelési listához");
            Display.DisplayProductsToOrder(KeszletMozgas.OutOfStock(KeszletMozgas.mozgasok), KeszletMozgas.GetProductAmounts(KeszletMozgas.mozgasok));

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