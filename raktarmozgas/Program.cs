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
            public byte id;
            public byte ora;
            public byte perc;
            public string termek;
            public float mennyiseg;
            public double egysegAr;
            public MozgasTipus tipus;
            public string partner;

            public static List<RaktarMozgas> mozgas = new List<RaktarMozgas>();

            /*Feldarabolja a beolvasott sorokat a megadott elválasztó jel mentén, az értékeket a struktura változóiba tölti.*/
            public static RaktarMozgas CreateRaktarMozgas(string input)
            {
                string[] data = input.Split(";");

                RaktarMozgas raktarMozgas = new RaktarMozgas();
                raktarMozgas.id = byte.Parse(data[0]);
                string[] ido = data[1].Split(":");
                raktarMozgas.ora = byte.Parse(ido[0]);
                raktarMozgas.perc = byte.Parse(ido[1]);
                raktarMozgas.termek = data[2];
                raktarMozgas.mennyiseg = float.Parse(data[3]);
                raktarMozgas.egysegAr = double.Parse(data[4]);
                raktarMozgas.tipus = (MozgasTipus)Enum.Parse(typeof(MozgasTipus), data[5]);
                raktarMozgas.partner = data[6];

                return raktarMozgas;
            }
        }

        struct Partner
        {
            public int id;
            public string nev;
            public MozgasTipus tipus;
            public float mennyiseg;
            public double ertek;

            public static List<Partner> partnerek = new List<Partner>();

            /*Partnerek szerint csoportosítja a készletmozgást, összegzi az egy partnerre vetített áruk mennyiségét és értékét*/
            public static List<Partner> CreatePartner(List<RaktarMozgas> mozgas)
            {
                Partner partner = new Partner();
                partner.id = 1;
                partner.nev = RaktarMozgas.mozgas[0].partner;
                partner.tipus = RaktarMozgas.mozgas[0].tipus;
                partner.mennyiseg = RaktarMozgas.mozgas[0].mennyiseg;
                partner.ertek = RaktarMozgas.mozgas[0].egysegAr * mozgas[0].mennyiseg;

                partnerek.Add(partner);

                for (int i = 1; i < mozgas.Count; i++)
                {
                    int j = 0;
                    while (j < partnerek.Count && Partner.partnerek[j].nev != RaktarMozgas.mozgas[i].partner) j++;
                    if (j == partnerek.Count)
                    {
                        partner = new Partner();
                        partner.id = j + 1;
                        partner.nev = RaktarMozgas.mozgas[i].partner;
                        partner.tipus = RaktarMozgas.mozgas[i].tipus;
                        partner.mennyiseg = RaktarMozgas.mozgas[i].mennyiseg;
                        partner.ertek = RaktarMozgas.mozgas[i].egysegAr * mozgas[i].mennyiseg;
                        partnerek.Add(partner);
                    }
                    else
                    {
                        partner = new Partner();
                        partner.id = j;
                        partner.nev = RaktarMozgas.mozgas[i].partner;
                        partner.tipus = RaktarMozgas.mozgas[i].tipus;
                        partner.mennyiseg = Partner.partnerek[j].mennyiseg + RaktarMozgas.mozgas[i].mennyiseg;
                        partner.ertek = Partner.partnerek[j].ertek + RaktarMozgas.mozgas[i].egysegAr * mozgas[i].mennyiseg;
                        partnerek.RemoveAt(j);
                        partnerek.Add(partner);
                    }
                }

                return Partner.partnerek;
            }
        }

        struct TermekRendeles 
        { 
            public int id; 
            public byte ora;
            public byte perc;
            public string termek;
            public float mennyiseg;
            public string partner;

            public static List<TermekRendeles> rendeles = new List<TermekRendeles>();

            /*Összegzi a kifogyó termékeket és létrehozza a rendelési listát a megfelelő beszállítók hozzárendelésével*/
            public static List<TermekRendeles> CreateOrderList(List<RaktarMozgas> mozgas)
            {
                string[] termekek = OutOfStock(mozgas);
                float[,] mennyisegek = GetProductAmounts(mozgas);
                string[,] beszallitok = GetProductShippers(mozgas);

                int i = 0;
                while (i < termekek.Length && termekek[i] is not null)
                {
                    TermekRendeles termekRendeles = new TermekRendeles();
                    termekRendeles.id = i + 1;
                    termekRendeles.ora = 16;
                    termekRendeles.perc = 00;
                    termekRendeles.termek = termekek[i];
                    termekRendeles.mennyiseg = mennyisegek[i, (int)MozgasTipus.BESZERZES];
                    termekRendeles.partner = beszallitok[i, (int)MozgasTipus.BESZERZES];

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

        /*Kiszámolja és kiírja a legnagyobb értékben és mennyiségben szállító partner nevét.*/
        static void MaxTransport(List<RaktarMozgas> mozgas)
        {
            Partner.partnerek = Partner.CreatePartner(mozgas);

            var maxM = Partner.partnerek.MaxBy(m => m.mennyiseg);
            var maxE = Partner.partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség:\t{maxM.nev}\t{maxM.mennyiseg} kg\t{maxM.ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték:\t{maxE.nev}\t{maxE.mennyiseg} kg\t{maxE.ertek} Ft.");
        }

        /*Kiszámolja és kiírja az össz napi forgalom mennyiségét és értékét.*/
        static void SumTradeFlow(List<Partner> partnerek)
        {
            float[] osszmenny = new float[4];
            double[] osszertek = new double[4];

            foreach (var item in Partner.partnerek)
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

        /*Létrehoz egy tömböt a különböző féle termékek számára.*/
        static string[] GetProducts(List<RaktarMozgas> mozgas)
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
        static float[,] GetProductAmounts(List<RaktarMozgas> mozgas)
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

        /*Létrehoz egy kétdimenziós tömböt a termékek beszerzési és eladási ára számára.*/
        static double[,] GetProductPrices(List<RaktarMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            double[,] termekArak = new double[termekFajta, mozgasFajta];

            foreach (var item in mozgas) 
            { 
                int j = 0;
                while (j < termekek.Length && !(item.termek == termekek[j])) j ++;
                termekArak[j, (int)item.tipus] = item.egysegAr;
            }

            return termekArak;
        }

        /*Létrehoz egy tömböt az egyes termékek beszállítói számára*/
        static string[,] GetProductShippers(List<RaktarMozgas> mozgas)
        {
            string[] termekek = GetProducts(mozgas);
            string[,] beszallitok = new string[termekFajta, mozgasFajta];

            foreach (var item in mozgas)
            {
                int j = 0;
                while (j < termekek.Length && !(item.termek == termekek[j])) j ++;
                beszallitok[j, (int)item.tipus] = item.partner;
            }

            return beszallitok;
        }

        /*Kiszámolja mely termékek készlete esett 50% alá, és visszaadja azon termékeket, melyekből rendelést kell leadni.*/
        static string[] OutOfStock(List<RaktarMozgas> mozgas)
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
                    j ++;
                }
            }

            return kifogyoTermekek;
        }

        /*Kilistázza azon termékek mennyiségeit, melyek készlete 50% alá esett.*/
        static void ProductsToOrder(string[] termekek, float[,] mennyisegek)
        {
            int k = 0;

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
            string[] termekek = GetProducts(RaktarMozgas.mozgas);
            float[,] mennyisegek = GetProductAmounts(RaktarMozgas.mozgas);
            double[,] termekArak = GetProductPrices(RaktarMozgas.mozgas);

            double osszKiadas = 0;
            double osszBevetel = 0;
            double osszProfit = 0;

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

        /*Óránként összegzi a ki- és bemenő forgalmakat.*/
        static int[,] SalesPerHour(List<RaktarMozgas> mozgas)
        {
            int[,] forgalmak = new int[24, mozgasFajta];

            foreach (var item in RaktarMozgas.mozgas)
            {
                forgalmak[item.ora, (int)item.tipus]++;
            }

            return forgalmak;
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
         kérhetjük az időpontokat, amíg egy "üres" Enter-t, vagy nem numerikus billentyűt nem nyomunk. Ha 24 óránál nagyobb időpontot 
        írunk be, rendszerfigyelmeztetést kapunk, ami után folytathatjuk az órák szerinti lekérdezéseket.*/
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
            MaxTransport(RaktarMozgas.mozgas);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");
            SumTradeFlow(Partner.partnerek);

            Console.WriteLine("\n<50%\tTermék\t\tRend.\tEladás\tKészlet");
            ProductsToOrder(OutOfStock(RaktarMozgas.mozgas), GetProductAmounts(RaktarMozgas.mozgas));

            Console.WriteLine("\nFájlba kiírt megrendelés ellenőrzése");
            PrintOrderList(TermekRendeles.CreateOrderList(RaktarMozgas.mozgas), "rendeles.txt");
            LoadFile("rendeles.txt", "display");

            Console.WriteLine("\nNapi\tTermékeladás\tBevétel\tKiadás\tHaszon");
            DailyProfit();

            Console.WriteLine($"\nNapi legforgalmasabb időszakok órák szerint");
            MaxTradeFlow(SalesPerHour(RaktarMozgas.mozgas), 8, 16);

            Console.WriteLine("\nAdja meg, hogy mely órák forgalmi adatait szeretné lekérdezni! (Kilépés: Enter)");
            HourlySales(SalesPerHour(RaktarMozgas.mozgas));
        }
    }
}