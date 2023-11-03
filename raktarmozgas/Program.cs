using System.Collections.Generic;
using System.Security.Principal;

namespace raktarmozgas
{
    internal class Program
    {
        static int termekFajta = 10; //Termékféleségek aktuális darabszáma
        static int mozgasFajta = 4; //Készletmozgás lehetséges típusai

        enum MozgasTipus
        {
            BESZERZES,
            ELADAS,
            VISSZARU,
            SELEJT
        }
        struct RaktarMozgas
        {
            byte id;
            byte ora;
            byte perc;
            string termek;
            float mennyiseg;
            double egysegAr;
            MozgasTipus tipus;
            public string partner;

            public string Termek { get => termek; }
            public float Mennyiseg {  get => mennyiseg; }
            public double EgysegAr { get => egysegAr; }
            public MozgasTipus Tipus { get => tipus; }

            public static List<RaktarMozgas> mozgas = new();

            /*Feldarabolja a beolvasott sorokat a megadott elválasztó jel mentén, az értékeket a struktura változóiba tölti.*/
            public static RaktarMozgas CreateRaktarMozgas(string input)
            {
                string[] data = input.Split(";");
                string[] ido = data[1].Split(":");

                RaktarMozgas raktarMozgas = new()
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
            public static string[] GetProducts(List<RaktarMozgas> mozgas)
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

            /*Létrehoz egy kétdimenziós tömböt a beszállított és eladott termékek mennyiségei számára.*/
            public static float[,] GetProductAmounts(List<RaktarMozgas> mozgas)
            {
                string[] termekek = RaktarMozgas.GetProducts(mozgas);
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

            /*Létrehoz egy kétdimenziós tömböt a termékek beszerzési és eladási ára számára.*/
            public static double[,] GetProductPrices(List<RaktarMozgas> mozgas)
            {
                string[] termekek = RaktarMozgas.GetProducts(mozgas);
                double[,] termekArak = new double[termekFajta, mozgasFajta];

                foreach (var item in mozgas)
                {
                    int j = 0;
                    while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                    termekArak[j, (int)item.tipus] = item.egysegAr;
                }

                return termekArak;
            }

            /*Létrehoz egy tömböt az egyes termékek beszállítói számára*/
            public static string[,] GetProductShippers(List<RaktarMozgas> mozgas)
            {
                string[] termekek = RaktarMozgas.GetProducts(mozgas);
                string[,] beszallitok = new string[termekFajta, mozgasFajta];

                foreach (var item in mozgas)
                {
                    int j = 0;
                    while (j < termekek.Length && !(item.termek == termekek[j])) j++;
                    beszallitok[j, (int)item.tipus] = item.partner;
                }

                return beszallitok;
            }

            /*Óránként összegzi a ki- és bemenő forgalmakat.*/
            public static int[,] SalesPerHour(List<RaktarMozgas> mozgas)
            {
                int[,] forgalmak = new int[24, mozgasFajta];

                foreach (var item in mozgas)
                {
                    forgalmak[item.ora, (int)item.tipus]++;
                }

                return forgalmak;
            }
        }

        struct Partner
        {
            int id = 0;
            public string nev = "";
            public MozgasTipus tipus = 0;
            public float mennyiseg = 0;
            public double ertek = 0;

            public static List<Partner> partnerek = new List<Partner>();

            public Partner() { }

            /*A konstruktor metódus overloadjában létrehoz két Partner példányt, melyek az adatok betöltése után visszaadják a legnagyobb
             beszállítók adatait.*/
            public Partner(List<Partner> partnerek, string arg = "mennyiseg")
            {
                Partner max = (arg == "ertek") ? partnerek.MaxBy(e => e.ertek) : partnerek.MaxBy(m => m.mennyiseg);
                id = max.id;
                nev = max.nev;
                MozgasTipus tipus = max.tipus;
                mennyiseg = max.mennyiseg;
                ertek = max.ertek;
            }

            /*Partnerek szerint csoportosítja a készletmozgást, összegzi az egy partnerre vetített áruk mennyiségét és értékét*/
            public static List<Partner> CreatePartner(List<RaktarMozgas> mozgas)
            {
                Partner partner = new Partner()
                {
                    id = 1,
                    nev = RaktarMozgas.mozgas[0].partner,
                    tipus = RaktarMozgas.mozgas[0].Tipus,
                    mennyiseg = RaktarMozgas.mozgas[0].Mennyiseg,
                    ertek = RaktarMozgas.mozgas[0].EgysegAr * mozgas[0].Mennyiseg
                };

                partnerek.Add(partner);

                for (int i = 1; i < mozgas.Count; i++)
                {
                    int j = 0;
                    while (j < partnerek.Count && partnerek[j].nev != RaktarMozgas.mozgas[i].partner) j++;
                    if (j == partnerek.Count)
                    {
                        partner = new Partner()
                        {
                            id = j + 1,
                            nev = RaktarMozgas.mozgas[i].partner,
                            tipus = RaktarMozgas.mozgas[i].Tipus,
                            mennyiseg = RaktarMozgas.mozgas[i].Mennyiseg,
                            ertek = RaktarMozgas.mozgas[i].EgysegAr * mozgas[i].Mennyiseg
                        };

                        partnerek.Add(partner);
                    }
                    else
                    {
                        partner = new Partner()
                        {
                            id = j,
                            nev = RaktarMozgas.mozgas[i].partner,
                            tipus = RaktarMozgas.mozgas[i].Tipus,
                            mennyiseg = partnerek[j].mennyiseg + RaktarMozgas.mozgas[i].Mennyiseg,
                            ertek = partnerek[j].ertek + RaktarMozgas.mozgas[i].EgysegAr * mozgas[i].Mennyiseg
                        };

                        partnerek.RemoveAt(j);
                        partnerek.Add(partner);
                    }
                }

                return partnerek;
            }
            //List<Partner>  partners = CreatePartner(RaktarMozgas.mozgas);
            //public static Partner maxM = partnerek.MaxBy(m => m.mennyiseg);
            //public static Partner maxE = partnerek.MaxBy(e => e.ertek);
        }   

        struct TermekRendeles 
        { 
            public int id; 
            public byte ora;
            public byte perc;
            public string termek;
            public float mennyiseg;
            public string partner;

            public static List<TermekRendeles> rendeles = new();

            /*Összegzi a kifogyó termékeket és létrehozza a rendelési listát a megfelelő beszállítók hozzárendelésével*/
            public static List<TermekRendeles> CreateOrderList(List<RaktarMozgas> mozgas)
            {
                string[] termekek = OutOfStock(mozgas);
                float[,] mennyisegek = RaktarMozgas.GetProductAmounts(mozgas);
                string[,] beszallitok = RaktarMozgas.GetProductShippers(mozgas);

                int i = 0;
                while (i < termekek.Length && termekek[i] is not null)
                {
                    TermekRendeles termekRendeles = new()
                    {
                        id = i + 1,
                        ora = 16,
                        perc = 00,
                        termek = termekek[i],
                        mennyiseg = mennyisegek[i, (int)MozgasTipus.BESZERZES],
                        partner = beszallitok[i, (int)MozgasTipus.BESZERZES]
                    };
                    rendeles.Add(termekRendeles);
                    i++;
                }
                return rendeles;
            }
        }

        /*Beolvassa és eltárolja a fájl tartalmát, ellenőrzi, hogy a fájlban vannak-e hibák, illetve üres sorok, amiről üzenetet is küld.*/
        static void LoadFile(string path, string output)
        {
            StreamReader sr = new StreamReader(path);
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
                            RaktarMozgas.mozgas.Add(RaktarMozgas.CreateRaktarMozgas(row));
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

        /*Kiírja a legnagyobb értékben és mennyiségben szállító partner nevét. (A komment zárójelek eltávolításával, és az első két sor,
         * valamint a Partner struktura konstruktorának kikommentelésével itt is meg lehet valósítani a max/min kiválasztás műveletét.)*/
        static void MaxTransport(List<Partner> partnerek)
        {
            Partner maxE = new Partner(partnerek, "ertek");
            Partner maxM = new Partner(partnerek, "mennyiseg");
            //Partner.partnerek = Partner.CreatePartner(mozgas);

            //Partner maxM = Partner.partnerek.MaxBy(m => m.mennyiseg);
            //var maxE = Partner.partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség:\t{maxM.nev}\t{maxM.mennyiseg} kg\t{maxM.ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték:\t{maxE.nev}\t{maxE.mennyiseg} kg\t{maxE.ertek} Ft.");
        }
        
        /*Kiszámolja és kiírja az össz napi forgalom mennyiségét és értékét.*/
        static void SumTradeFlow(List<Partner> partnerek)
        {
            float[] osszmenny = new float[4];
            double[] osszertek = new double[4];

            foreach (var item in partnerek)
            {
                osszmenny[(int)item.tipus] += item.mennyiseg;
                osszertek[(int)item.tipus] += item.ertek;
            }

            float osszMennyB = osszmenny[(int)MozgasTipus.BESZERZES];
            float osszMennyE = osszmenny[(int)MozgasTipus.ELADAS];
            double osszErtekB = osszertek[(int)MozgasTipus.BESZERZES];
            double osszErtekE = osszertek[(int)MozgasTipus.ELADAS];

            Console.WriteLine($"\tBeszállított termékek\tmennyisége: {osszMennyB} kg\tösszértéke: {Math.Round(osszErtekB)} Ft.");
            Console.WriteLine($"\tEladott termékek\tmennyisége: {osszMennyE} kg\tösszértéke: {Math.Round(osszErtekE)} Ft.");
        }

        /*Kiszámolja mely termékek készlete esett 50% alá, és visszaadja azon termékeket, melyekből rendelést kell leadni.*/
        static string[] OutOfStock(List<RaktarMozgas> mozgas)
        {
            string[] termekek = RaktarMozgas.GetProducts(mozgas);
            float[,] mennyisegek = RaktarMozgas.GetProductAmounts(mozgas);
            string[] kifogyoTermekek = new string[termekFajta];
            int j = 0;

            for (int i = 0; i < termekek.Length; i++)
            {
                if (mennyisegek[i, (int)MozgasTipus.ELADAS] > mennyisegek[i, (int)MozgasTipus.BESZERZES] / 2)
                {
                    kifogyoTermekek[j] = termekek[i];
                    j ++;
                }
            }

            return kifogyoTermekek;
        }

        /*Kilistázza azon termékek mennyiségeit, melyek készlete 50% alá esett.*/
        static void ProductsToOrder(string[] termekek, float[,] mennyisegek)
        {
            int k = 0;

            Console.WriteLine("\n\tTermék\t\tRend.\tEladás\tKészlet");

            while (k < termekek.Length && termekek[k]is not null)
            {
                float beszerzes = mennyisegek[k, (int)MozgasTipus.BESZERZES];
                float eladas = mennyisegek[k, (int)MozgasTipus.ELADAS];
                float keszlet = beszerzes - eladas;

                Console.WriteLine($"\t{termekek[k]}\t{beszerzes}\t{eladas}\t{keszlet} kg.");
                k++;
            }
        }

        /*Kiírja fájlba a rendelési listát*/
        static void PrintOrderList(List<TermekRendeles> rendeles, string path)
        {
            StreamWriter sr = new StreamWriter(path);

            sr.WriteLine("az;ido;rend_termek;menny;partner");

            foreach (var item in rendeles)
            {
                sr.WriteLine($"{item.id};{item.ora}:{item.perc};{item.termek};{item.mennyiseg};{item.partner}");
            }
            sr.Close();
            
        }

        /*Termékenként összegzi a napi bevételt és kiadást, majd ebből kiszámolja és kiírja a profitot.*/
        static void DailyProfit()
        {
            string[] termekek = RaktarMozgas.GetProducts(RaktarMozgas.mozgas);
            float[,] mennyisegek = RaktarMozgas.GetProductAmounts(RaktarMozgas.mozgas);
            double[,] termekArak = RaktarMozgas.GetProductPrices(RaktarMozgas.mozgas);

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
            MaxTransport(Partner.CreatePartner(RaktarMozgas.mozgas));
            //Partner maxM = new Partner(Partner.CreatePartner(RaktarMozgas.mozgas));
            //Console.WriteLine(maxM.nev);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");
            SumTradeFlow(Partner.partnerek);

            Console.WriteLine("\nA készlet 50%-a alá eső termékek, melyek automatikusan hozzáadódnak a rendelési listához");
            ProductsToOrder(OutOfStock(RaktarMozgas.mozgas), RaktarMozgas.GetProductAmounts(RaktarMozgas.mozgas));

            Console.WriteLine("\nFájlba kiírt és onnan visszaolvasott rendelési lista ellenőrzése");
            PrintOrderList(TermekRendeles.CreateOrderList(RaktarMozgas.mozgas), "rendeles.txt");
            LoadFile("rendeles.txt", "display");

            Console.WriteLine("\nA napi termékeladásból származó üzleti haszon (eladott mennyiség x (eladási ár - beszerzési ár))");
            DailyProfit();

            Console.WriteLine($"\nNapi legforgalmasabb időszakok órák szerint (Lehetnek a megjelenítettel azonos forgalmú időszakok!)");
            MaxTradeFlow(RaktarMozgas.SalesPerHour(RaktarMozgas.mozgas), 8, 16);

            Console.WriteLine("\nAdja meg, hogy mely órák forgalmi adatait szeretné lekérdezni! (Kilépés: Enter)");
            HourlySales(RaktarMozgas.SalesPerHour(RaktarMozgas.mozgas));
        }
    }
}