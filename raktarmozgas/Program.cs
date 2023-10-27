using System.Security.Principal;

namespace raktarmozgas
{
    internal class Program
    {
        enum MozgasTipus
        {
            VASARLAS,
            ELADAS
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

        struct Beszallito
        {
            public string partner;
            public MozgasTipus tipus;
            public float mennyiseg;
            public double ertek;
        }

        static List<RaktarMozgas> mozgas = new List<RaktarMozgas>();

        static RaktarMozgas Convert(string input)
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
                    mozgas.Add(Convert(row));
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
        static void Main(string[] args)
        {
            LoadFile("raktarstat.log");


            Console.WriteLine("\nLegtöbbet, és legnagyobb értékben szállító partner");

            List<Beszallito> beszallitok = new List<Beszallito>();
            
            Beszallito beszallito = new Beszallito();
            beszallito.partner = mozgas[0].partner;
            beszallito.tipus = mozgas[0].tipus;
            beszallito.mennyiseg = mozgas[0].mennyiseg;
            beszallito.ertek = mozgas[0].egysegAr * mozgas[0].mennyiseg;

            beszallitok.Add(beszallito);

            for (int i = 1; i < mozgas.Count; i++)
            {
                int j = 0;
                while (j < beszallitok.Count && beszallitok[j].partner != mozgas[i].partner)
                {
                    j++;
                }
                if (j == beszallitok.Count)
                {
                    beszallito = new Beszallito();
                    beszallito.partner = mozgas[i].partner;
                    beszallito.tipus = mozgas[i].tipus;
                    beszallito.mennyiseg = mozgas[i].mennyiseg;
                    beszallito.ertek = mozgas[i].egysegAr * mozgas[i].mennyiseg;
                    beszallitok.Add(beszallito);
                }
                else
                {
                    beszallito = new Beszallito();
                    beszallito.partner = mozgas[i].partner;
                    beszallito.tipus = mozgas[i].tipus;
                    beszallito.mennyiseg = beszallitok[j].mennyiseg + mozgas[i].mennyiseg;
                    beszallito.ertek = beszallitok[j].ertek + mozgas[i].egysegAr * mozgas[i].mennyiseg;

                    beszallitok.RemoveAt(j);
                    beszallitok.Add(beszallito);
                }
            }

            int k = 0;
            do { k++;} while (k < beszallitok.Count && beszallitok[k].tipus != MozgasTipus.VASARLAS);
            
            int iOfMaxMennyiseg = k;
            float maxMennyiseg = beszallitok[k].mennyiseg;
            int iOfMaxErtek = k;
            double maxErtek = beszallitok[k].ertek;

            while (k < beszallitok.Count)
            {
                if (beszallitok[k].tipus == MozgasTipus.VASARLAS && beszallitok[k].mennyiseg > maxMennyiseg)
                {
                    maxMennyiseg = beszallitok[k].mennyiseg;
                    iOfMaxMennyiseg = k;
                }

                if (beszallitok[k].tipus == MozgasTipus.VASARLAS && beszallitok[k].ertek > maxErtek)
                {
                    maxErtek = beszallitok[k].ertek;
                    iOfMaxErtek = k;
                }
                //Console.WriteLine($"\t{beszallitok[k].partner}: \t{beszallitok[k].mennyiseg} kg, \t{beszallitok[k].ertek} Ft.");
                k++;
            }

            Console.WriteLine($"\tLegnagyobb mennyiség: \n\t\t{beszallitok[iOfMaxMennyiseg].partner} - {beszallitok[iOfMaxMennyiseg].mennyiseg} kg, {beszallitok[iOfMaxMennyiseg].ertek} Ft.");
            Console.WriteLine($"\tLegnagyobb érték: \n\t\t{beszallitok[iOfMaxErtek].partner} - {beszallitok[iOfMaxErtek].mennyiseg} kg, {beszallitok[iOfMaxErtek].ertek} Ft.");

            double osszErtek = 0;

            foreach (var item in beszallitok)
            {
                if (item.tipus == MozgasTipus.VASARLAS)
                {
                    osszErtek += item.ertek;
                }
            }

            Console.WriteLine($"\nBeszállított termékek összértéke: \n\t{Math.Round(osszErtek)} Ft.");

            osszErtek = 0;

            foreach (var item in beszallitok)
            {
                if (item.tipus == MozgasTipus.ELADAS)
                {
                    osszErtek += item.ertek;
                }
            }

            Console.WriteLine($"\nEladott termékek összértéke: \n\t{Math.Round(osszErtek)} Ft.");

            Console.WriteLine($"\nLegforgalmasabb időszakok");

            int[] forgalom = new int[24];

            foreach (var b in mozgas)
            {
                forgalom[b.ora]++;
            }

            int maxForgalom = forgalom[8];
            int iOfMaxForgalom = 8;

            for (var i = 9;  i < 24; i++)
            {
                if (forgalom[i] > maxForgalom)
                {
                    maxForgalom = forgalom[i];
                    iOfMaxForgalom = i;
                }
            }

            Console.WriteLine($"\t{iOfMaxForgalom} óra: {maxForgalom} db. forgalom (vásárlás/eladás)");

            int[] vasarlas = new int[24];

            foreach (var b in mozgas)
            {
                if(b.tipus == MozgasTipus.VASARLAS)
                { 
                vasarlas[b.ora]++;
                }
            }

            int maxVasarlas = vasarlas[8];
            int iOfMaxVasarlas = 8;

            for (var i = 9; i < 24; i++)
            {
                if (vasarlas[i] > maxVasarlas)
                {
                    maxVasarlas = vasarlas[i];
                    iOfMaxVasarlas = i;
                }
            }

            Console.WriteLine($"\t{iOfMaxVasarlas} óra: {maxVasarlas} db. vásárlás");

            int[] eladas = new int[24];

            foreach (var b in mozgas)
            {
                if (b.tipus == MozgasTipus.ELADAS)
                {
                    eladas[b.ora]++;
                }
            }

            int maxEladas = eladas[8];
            int iOfMaxEladas = 8;

            for (var i = 9; i < 24; i++)
            {
                if (eladas[i] > maxEladas)
                {
                    maxEladas = eladas[i];
                    iOfMaxEladas = i;
                }
            }
            while (Console.ReadKey(ConsoleKey.Enter);

            Console.WriteLine($"\t{iOfMaxEladas} óra: {maxEladas} db. eladás");
        }
    }
}