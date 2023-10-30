﻿using System.Security.Principal;

namespace raktarmozgas
{
    internal class Program
    {
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
        }

        struct Partner
        {
            public int id;
            public string nev;
            public MozgasTipus tipus;
            public float mennyiseg;
            public double ertek;
        }

        static List<RaktarMozgas> mozgas = new List<RaktarMozgas>();

        static List<Partner> partnerek = new List<Partner>();

        /*Feldarabolja a beolvasott sorokat a megadott elválasztó jel mentén, az értékeket struktura változóiba tölti.*/
        static RaktarMozgas ConvertRowToStruct(string input)
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

        /*Beolvassa és eltárolja a fájl tartalmát, ellenőrzi, hogy a fájlban vannak-e hibák, illetve üres sorok, amiről üzenetet is küld.*/
        static void LoadFile(string path)
        {
            StreamReader sr = new StreamReader(path);
            string[] header = sr.ReadLine().Split(";");

            while (!sr.EndOfStream)
            {
                string row = "";
                try
                {
                    row = sr.ReadLine();
                    mozgas.Add(ConvertRowToStruct(row));
                    //Console.WriteLine(row);
                    //Console.ReadLine();
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

        /*Partnerek szerint csoportosítja a készletmozgást, összegzi az egy partnerre vetített áruk mennyiségét és értékét*/
        static List<Partner> CreatePartner(List<RaktarMozgas> mozgas)
        {
            Partner partner = new Partner();
            partner.id = 1;
            partner.nev = mozgas[0].partner;
            partner.tipus = mozgas[0].tipus;
            partner.mennyiseg = mozgas[0].mennyiseg;
            partner.ertek = mozgas[0].egysegAr * mozgas[0].mennyiseg;

            partnerek.Add(partner);

            for (int i = 1; i < mozgas.Count; i++)
            {
                int j = 0;
                while (j < partnerek.Count && partnerek[j].nev != mozgas[i].partner) j++;
                if (j == partnerek.Count)
                {
                    partner = new Partner();
                    partner.id = j + 1;
                    partner.nev = mozgas[i].partner;
                    partner.tipus = mozgas[i].tipus;
                    partner.mennyiseg = mozgas[i].mennyiseg;
                    partner.ertek = mozgas[i].egysegAr * mozgas[i].mennyiseg;
                    partnerek.Add(partner);
                }
                else
                {
                    partner = new Partner();
                    partner.id = j;
                    partner.nev = mozgas[i].partner;
                    partner.tipus = mozgas[i].tipus;
                    partner.mennyiseg = partnerek[j].mennyiseg + mozgas[i].mennyiseg;
                    partner.ertek = partnerek[j].ertek + mozgas[i].egysegAr * mozgas[i].mennyiseg;
                    partnerek.RemoveAt(j);
                    partnerek.Add(partner);
                }
            }

            return partnerek;
        }

        /*Kiszámolja és kiírja a legnegyobb értékben és mennyiségben szállító partner nevét.*/
        static void MaxTransport(List<RaktarMozgas> mozgas)
        {
            partnerek = CreatePartner(mozgas);

            var maxM = partnerek.MaxBy(m => m.mennyiseg);
            var maxE = partnerek.MaxBy(m => m.ertek);

            Console.WriteLine($"\tLegnagyobb mennyiség: \n\t\t{maxM.nev} - {maxM.mennyiseg} kg, {maxM.ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték: \n\t\t{maxE.nev} - {maxE.mennyiseg} kg, {maxE.ertek} Ft.");
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

            Console.WriteLine($"\tBeszállított termékek mennyisége: {osszMennyB} kg, összértéke: {Math.Round(osszErtekB)} Ft.");
            Console.WriteLine($"\tEladott termékek mennyisége: {osszMennyE} kg, összértéke: {Math.Round(osszErtekE)} Ft.");
        }

        /*Kilistázz azon termékek mennyiségeit, melyek készlete 50% alá esett.*/
        static void ProductsToOrder(List<RaktarMozgas> mozgas)
        {
            string[] termekek = new string[30];
            float[,] mennyisegek = new float[30,4];
            int i = 0;

            foreach (var item in mozgas)
            {
                if (!termekek.Contains(item.termek))
                {
                    termekek[i] = item.termek;
                    i++;
                }
            }

            foreach (var termek in mozgas)
            {
                int j = 0;

                while (j < i && !(termek.termek == termekek[j])) j++;

                mennyisegek[j,(int)termek.tipus] += termek.mennyiseg;
            }

            for (int k = 0; k < i; k++)
            {
                float beszerzes = mennyisegek[k, (int)MozgasTipus.BESZERZES];
                float eladas = mennyisegek[k, (int)MozgasTipus.ELADAS];
                float keszlet = beszerzes - eladas;

                if (keszlet < eladas)
                {
                    Console.WriteLine($"\t{termekek[k]}: {beszerzes} - {eladas} = {keszlet} kg.");
                }
            }
        }

        /*Óránként összeszámolja ki- és bemenő forgalmakat.*/
        static int[,] SalesPerHour(List<RaktarMozgas> mozgas)
        {
            int[,] forgalmak = new int[24, 4];

            foreach (var item in mozgas)
            {
                forgalmak[item.ora, (int)item.tipus]++;
            }

            return forgalmak;
        }

        /*Kiszámolja és kiírja melyik órában volt a legnagyobb forgalom, forgalom típusa szerint is.*/
        static void MaxTradeFlow(List<RaktarMozgas> mozgas) 
        {
            int[,] forgalmak = SalesPerHour(mozgas);
            int[] forgalom = new int[24];

            foreach (var item in mozgas)
            {
                forgalom[item.ora]++;
            }

            int nyitas = 8;
            int zaras = 16;

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

                if (forgalmak[i, (int)MozgasTipus.ELADAS] > maxElad)
                {
                    iOfMaxE = i;
                    maxElad = forgalmak[iOfMaxE, (int)MozgasTipus.ELADAS];
                }
            }

            Console.WriteLine($"\t{iOfMaxF} óra: {maxForg} db. forgalom (beszerzés/eladás)");
            Console.WriteLine($"\t{iOfMaxB} óra: {maxBesz} db. beszerzés");
            Console.WriteLine($"\t{iOfMaxE} óra: {maxElad} db. eladás");
        }

        /*A felhasználótól bekért időpontok szerint kiszámolja és kiírja, hogy az adott órákban mennyi beszerzés és eladás volt. Addig 
         kérhetjük az időpontokat, amíg egy "üres" Enter-t, vagy nem numerikus billentyűt nem nyomunk. Ha 24 óránál nagyobb időpontot 
        írunk be, rendszerfigyelmeztetést kapunk, ami után folytathatjuk az órák szerinti lekérdezéseket.*/
        static void HourlySales(List<RaktarMozgas> mozgas)
        {
            int[,] forgalmak = SalesPerHour(mozgas);
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

            LoadFile("raktarstat.log");

            Console.WriteLine("\nLegtöbbet, és legnagyobb értékben szállító partner");

            MaxTransport(mozgas);

            Console.WriteLine("\nÖsszes beszállított és eladott termék mennyisége és összértéke");

            SumTradeFlow(partnerek);

            Console.WriteLine("\nTermékek, melyek készlete 50% alá esett");

            ProductsToOrder(mozgas);

            Console.WriteLine($"\nNapi legforgalmasabb időszakok órák szerint");

            MaxTradeFlow(mozgas);

            Console.WriteLine("\nAdja meg, hogy mely órák forgalmi adatait szeretné lekérdezni! (Kilépés: Enter)");

            HourlySales(mozgas);
            
        }
    }
}