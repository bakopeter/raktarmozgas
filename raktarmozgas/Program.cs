using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Xml.Linq;

namespace raktarmozgas
{
    internal class Program
    {
        static readonly int termekFajta = 10; //Termékféleségek aktuális darabszáma
        static readonly int mozgasFajta = 4; //Készletmozgás lehetséges típusainak száma

        enum MozgasTipus //A készletmozgás lehetséges típusai
        {
            BESZERZES,
            ELADAS,
            VISSZARU,
            SELEJT
        }
        struct KeszletMozgas //Egy készletmozgást reprezentáló struktúra
        {
            byte id;
            byte ora;
            byte perc;
            string termek;
            float mennyiseg;
            double egysegAr;
            MozgasTipus tipus;
            string partner;

            public float Mennyiseg {  get => mennyiseg; }
            public double EgysegAr { get => egysegAr; }
            public MozgasTipus Tipus { get => tipus; }
            public string Partner { get => partner; }

            public static List<KeszletMozgas> mozgasok = new(); //A készletmozgásokat reprezentáló struktúrák listája

            /*Feldarabolja a beolvasott sorokat a megadott elválasztó jel mentén, az értékeket a struktúra változóiba tölti.*/
            public static KeszletMozgas CreateRaktarMozgas(string input)
            {
                string[] data = input.Split(";");
                string[] ido = data[1].Split(":");

                KeszletMozgas raktarMozgas = new()
                {
                    id = byte.Parse(data[0]),
                    ora = byte.Parse(ido[0]),
                    perc = byte.Parse(ido[1]),
                    termek = data[2],
                    mennyiseg = float.Parse(data[3]),
                    egysegAr = double.Parse(data[4]),
                    tipus = (MozgasTipus)Enum.Parse(typeof(MozgasTipus), data[5]),
                    partner = data[6]
                };

                return raktarMozgas;
            }

            /*Létrehoz egy tömböt a különböző féle termékek számára.*/
            public static string[] GetProducts(List<KeszletMozgas> mozgas)
            {
                int i = 0;
                string[] termekek = new string[termekFajta];

                foreach (var item in mozgas)
                {
                    if (!termekek.Contains(item.termek))
                    {
                        termekek[i] = item.termek;
                        i++;
                    }
                }
                return termekek;
            }

            /*Létrehoz egy kétdimenziós tömböt a beszállított és eladott termékek mennyiségei számára (2. dimenzió a mozgástípus értéke).*/
            public static float[,] GetProductAmounts(List<KeszletMozgas> mozgas)
            {
                string[] termekek = GetProducts(mozgas);
                float[,] mennyisegek = new float[termekFajta, mozgasFajta];

                foreach (var item
                    in mozgas)
                {
                    int j = 0;
                    while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                    mennyisegek[j, (int)item.tipus] += item.mennyiseg;
                }
                return mennyisegek;
            }

            /*Létrehoz egy kétdimenziós tömböt a termékek beszerzési és eladási ára számára (2. dimenzió a mozgástípus értéke).*/
            public static double[,] GetProductPrices(List<KeszletMozgas> mozgas)
            {
                string[] termekek = GetProducts(mozgas);
                double[,] termekArak = new double[termekFajta, mozgasFajta];

                foreach (var item in mozgas)
                {
                    int j = 0;
                    while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                    termekArak[j, (int)item.tipus] = item.egysegAr;
                }
                return termekArak;
            }

            /*Létrehoz egy tömböt az egyes termékek beszállítói számára (2. dimenzió a mozgástípus értéke)*/
            public static string[,] GetProductShippers(List<KeszletMozgas> mozgas)
            {
                string[] termekek = GetProducts(mozgas);
                string[,] beszallitok = new string[termekFajta, mozgasFajta];

                foreach (var item in mozgas)
                {
                    int j = 0;
                    while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                    beszallitok[j, (int)item.tipus] = item.Partner;
                }
                return beszallitok;
            }

            /*Óránként összegzi a ki- és bemenő forgalmakat.*/
            public static int[,] SalesPerHour(List<KeszletMozgas> mozgas)
            {
                int[,] forgalmak = new int[24, mozgasFajta];

                foreach (var item in mozgas)
                {
                    forgalmak[item.ora, (int)item.tipus]++;
                }
                return forgalmak;
            }

            /*Kiszámolja mely termékek készlete esett 50% alá, és visszaadja azon termékeket, melyekből rendelést kell leadni.*/
            public static string[] OutOfStock(List<KeszletMozgas> mozgas)
            {
                string[] termekek = GetProducts(mozgas);
                float[,] mennyisegek = GetProductAmounts(mozgas);
                string[] kifogyoTermekek = new string[termekFajta];
                int j = 0;

                for (int i = 0; i < termekek.Length; i++)
                {
                    if (mennyisegek[i, (int)MozgasTipus.ELADAS] > mennyisegek[i, (int)MozgasTipus.BESZERZES] / 2)
                    {
                        kifogyoTermekek[j] = termekek[i];
                        j++;
                    }
                }
                return kifogyoTermekek;
            }
        }

        struct Partner //Egy ügyfelet reprezentáló struktúra
        {
            int id = 0;
            string nev = "";
            MozgasTipus tipus = 0;
            float mennyiseg = 0;
            double ertek = 0;

            public string Nev { get => nev; }
            public float Mennyiseg { get => mennyiseg; }
            public double Ertek { get => ertek; }

            public static List<Partner> partnerek = new(); //Az ügyfeleket reprezentáló struktúrák listája

            static Partner maxertek;
            public static Partner maxErtek
            {
                get => maxertek;
                set => maxertek = value;
            }
            public static Partner maxmenny;
            public static Partner maxMenny
            {
                get => maxmenny;
                set => maxmenny = value;
            }
            public Partner() { }

            /*A konstruktor metódus overloadjában létrehoz két Partner példányt, melyek az adatok betöltése után visszaadják a legnagyobb
             beszállítók adatait. (Az ügyfelek listájába nem töltődnek be!)*/
            Partner(List<Partner> partnerek, string arg = "mennyiseg")
            {
                Partner max = (arg == "ertek") ? partnerek.MaxBy(e => e.ertek) : partnerek.MaxBy(m => m.mennyiseg);
                id = max.id;
                nev = max.nev;
                MozgasTipus tipus = max.tipus;
                mennyiseg = max.mennyiseg;
                ertek = max.ertek;
            }

            /*Létrehozza az üzleti partnereket reprezentáló struktúrát.*/
            static Partner CreatePartner(List<KeszletMozgas> mozgas, int index, int id) 
            {
                Partner partner = new()
                {
                    id = id+1,
                    nev = mozgas[index].Partner,
                    tipus = mozgas[index].Tipus,
                    mennyiseg = mozgas[index].Mennyiseg,
                    ertek = mozgas[index].EgysegAr * mozgas[index].Mennyiseg
                };
                return partner; 
            }

            /*Ha ugyanazon partnerhez kapcsolódó eseményt talál, összegzi a partnerhez kötődő árumogás mennyiségét és értékét, majd
             frissíti a partnert reprezentáló struktúrát az aktuális értékekkel.*/
            static Partner UpdatePartner(List<KeszletMozgas> mozgas, int index, int id)
            {
                Partner partner = new()
                {
                    id = id,
                    nev = mozgas[index].Partner,
                    tipus = mozgas[index].Tipus,
                    mennyiseg = partnerek[id].mennyiseg + mozgas[index].Mennyiseg,
                    ertek = partnerek[id].ertek + mozgas[index].EgysegAr * mozgas[index].Mennyiseg
                };
                return partner;
            }

            /*Partnerek szerint csoportosítja a készletmozgást, összegzi az egy üzleti partnerre vetített áruk mennyiségét és értékét.
             Ezután a konstruktor overload metódusának meghívásával kiválasztja a legnagyobb mennyiségben és értékben szállító partnereket.*/
            public static List<Partner> CreatePartnerList(List<KeszletMozgas> mozgas)
            {
               partnerek.Add(CreatePartner(mozgas, 0, 0));

                for (int i = 1; i < mozgas.Count; i++)
                {
                    int j = 0;
                    while (j < partnerek.Count && partnerek[j].nev != mozgas[i].Partner) j++;
                    if (j == partnerek.Count) { partnerek.Add(CreatePartner(mozgas, i, j)); }
                    else { partnerek.Add(UpdatePartner(mozgas, i, j)); partnerek.RemoveAt(j); }
                }

                maxertek = new Partner(partnerek, "ertek");
                maxmenny = new Partner(partnerek, "mennyiseg");

                return partnerek;
            }

            /*Kiszámolja az össz napi termékforgalom mennyiségét.*/
            public static float[] SumTradeFlow(List<Partner> partnerek)
            {
                float[] osszmenny = new float[mozgasFajta];

                foreach (var item in partnerek)
                {
                    osszmenny[(int)item.tipus] += item.mennyiseg;
                }
                return osszmenny;
            }
            /*Kiszámoljaaz össz napi pénzforgalom értékét.*/
            public static double[] SumCashFlow(List<Partner> partnerek)
            {
                double[] osszertek = new double[mozgasFajta];

                foreach (var item in partnerek)
                {
                    osszertek[(int)item.tipus] += item.ertek;
                }
                return osszertek;
            }
        }   

        struct Termek //Egy termék állapotát reprezentáló struktúra
        { 
            int id; 
            byte ora;
            byte perc;
            MozgasTipus tipus;
            string nev;
            float mennyiseg;
            string partner;

            public static List<Termek> keszlet = new(); //A termékeket reprezentáló struktúrák listája

            /*Létrehozza a terméket reprezentáló struktúrát*/
            public static Termek CreateProduct(List<KeszletMozgas> mozgas, string[] termekek, int index)
            {
                float[,] mennyisegek = KeszletMozgas.GetProductAmounts(mozgas);
                string[,] beszallitok = KeszletMozgas.GetProductShippers(mozgas);

                Termek termek = new()
                {
                    id = index + 1,
                    ora = 16,
                    perc = 00,
                    tipus = MozgasTipus.BESZERZES,
                    nev = termekek[index],
                    mennyiseg = mennyisegek[index, (int)MozgasTipus.BESZERZES],
                    partner = beszallitok[index, (int)MozgasTipus.BESZERZES]
                };

                return termek;
            }

            /*Összegzi a kifogyó termékeket és létrehozza a rendelési listát a megfelelő beszállítók hozzárendelésével*/
            public static List<Termek> CreateOrderList(List<KeszletMozgas> mozgas)
            {
                string[] termekek = KeszletMozgas.OutOfStock(mozgas);
                
                int i = 0;
                while (i < termekek.Length && termekek[i] is not null)
                {
                    keszlet.Add(CreateProduct(mozgas, termekek, i));
                    i++;
                }
                return keszlet;
            }

            /*Létrehoz egy sort a rendelési listába a rendelési tétel adataiból*/
            public static string CollectProductDetails(Termek termek)
            {
                string termekAdatok = $"{termek.id};{termek.ora}:{termek.perc};{termek.nev};{termek.mennyiseg};{termek.partner}";

                return termekAdatok;
            }

        }

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
                        case "raktarMozgas":
                            KeszletMozgas.mozgasok.Add(KeszletMozgas.CreateRaktarMozgas(row));
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
        static void MaxTransport(List<Partner> partnerek, Partner maxMenny, Partner maxErtek)
        {
            //Partner.partnerek = Partner.CreatePartnerList(mozgas);

            //Partner maxM = Partner.partnerek.MaxBy(m => m.mennyiseg);
            //var maxE = Partner.partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség:\t{maxMenny.Nev}\t{maxMenny.Mennyiseg} kg\t{maxMenny.Ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték:\t{maxErtek.Nev}\t{maxErtek.Mennyiseg} kg\t{maxErtek.Ertek} Ft.");
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
        static void ProductsToOrder(string[] kifogyoTermekek, float[,] mennyisegek)
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
            LoadFile("raktarstat.log", "raktarMozgas");

            Console.WriteLine("\nLegtöbbet, és legnagyobb értékben szállító partner");
            MaxTransport(Partner.CreatePartnerList(KeszletMozgas.mozgasok), Partner.maxMenny, Partner.maxErtek);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");
            DisplayTradeFlow(Partner.SumTradeFlow(Partner.partnerek), Partner.SumCashFlow(Partner.partnerek));

            Console.WriteLine("\nA készlet 50%-a alá eső termékek, melyek automatikusan hozzáadódnak a rendelési listához");
            ProductsToOrder(KeszletMozgas.OutOfStock(KeszletMozgas.mozgasok), KeszletMozgas.GetProductAmounts(KeszletMozgas.mozgasok));

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