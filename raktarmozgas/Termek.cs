using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raktarmozgas
{
    internal class Termek : KeszletMozgas //Egy termék állapotát reprezentáló struktúra
    {
        int id;
        byte ora;
        byte perc;
        MozgasTipus tipus;
        string nev = "";
        float mennyiseg;
        string partner ="";

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
}
