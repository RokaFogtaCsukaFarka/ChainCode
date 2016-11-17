using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace SFX3NR
{
    class Logika
    {
        String[] fajl;
        int[] dimenziok;
        String[] matrix;
        Boolean nincsFekete;
        int[] elsoFekete;
        int[] szelsoErtekek;            //minN, maxN, minM, maxM
        Boolean egyFeketeFoltVan;
        int leghosszabbFeketeUt;
        int leghosszabbFeherUt;
        int[,] voltMarFekete;
        List<int[,]> voltMarFeher;
        String[] kimenet;
        enum hibaKod { OK, NINCSFAJL, URESFAJL, DIMENZIO, FOLTHIBA, FEHERFOLT };

        public Logika()
        {
            egyFeketeFoltVan = true;
            nincsFekete = false;
            dimenziok = new int[2];
            leghosszabbFeketeUt = 0;
            leghosszabbFeherUt = 0;
        }

        public int fajlBeolvasas(Int32 sorszam)
        {
            if (!File.Exists(Environment.CurrentDirectory + "\\fajlok\\FOLT" + sorszam + ".BE.txt"))
            {
                return (int)hibaKod.NINCSFAJL;   
            }

            fajl = System.IO.File.ReadAllLines(Environment.CurrentDirectory + "\\fajlok\\FOLT" + sorszam + ".BE.txt");

            if (fajl.Length == 0)
                return (int)hibaKod.URESFAJL;

            int valosN = fajl.Length - 1;
            int valosM = fajl[1].Length;

            //A bemeneti fajlban a 10-nel kisebb szamokat 05 formában kell megadni.
            dimenziok[0] = Int32.Parse(fajl[0][0].ToString() + fajl[0][1].ToString());
            dimenziok[1] = Int32.Parse(fajl[0][3].ToString() + fajl[0][4].ToString());

            // Ha a dimenzio jelzesek nem egyeznek meg a valossal vagy kisebb a tomb valamelyik dimenzioja vagy nagyobb mint 100
            if (dimenziok[0] != valosN || dimenziok[1] != valosM ||
                    valosN < 1 || valosN > 100 || valosM < 1 || valosM > 100)
                return (int)hibaKod.DIMENZIO;

            matrix = new String[dimenziok[0]];

            matrixFeltoltes(fajl);

            //Mindenhova maximum ketszer juthat el a bejaras soran (a harmadik mar redundans -> kilep)
            leghosszabbFeketeUt = 2 * initFeketekSzama();

            // A leghosszabb ut egy n*m-es matrixban a megadott szabalyok szerint megalkotott labirintusban a kacskaringo
            // Ennek palyajat max ketszer jarhatjuk be a program soran.
            leghosszabbFeherUt += dimenziok[1] - 3;
            for (int k = 5; k < (dimenziok[0] < dimenziok[1] ? dimenziok[0] : dimenziok[1]); k += 2)
            {
                leghosszabbFeherUt += dimenziok[0] - k + dimenziok[1] - k;
            }
            leghosszabbFeherUt *= 2;

            voltMarFekete = BufferKezeles.initBuffer(leghosszabbFeketeUt);
            if (!konzisztensAFolt())
                return (int)hibaKod.FOLTHIBA;

            voltMarFeher = new List<int[,]>();
            if (vanFeherFolt())
                return (int)hibaKod.FEHERFOLT;

            return (int)hibaKod.OK;
        }

        private void matrixFeltoltes(string[] fajl)
        {
            int elsoX = dimenziok[1];
            int utolsoX = 0;
            Boolean eddigNincsFekete = true;

            szelsoErtekek = new int[4] { 0, 0, dimenziok[1], 0 };
            for (int i = 1; i <= dimenziok[0]; i++)
            {
                matrix[i - 1] = fajl[i];

                if (fajl[i].Contains('x'))
                {
                    //minN
                    if (szelsoErtekek[0] == 0)
                        szelsoErtekek[0] = i - 1;
                    //maxN
                    else
                        szelsoErtekek[1] = i - 1;
                    //minM
                    elsoX = fajl[i].IndexOf('x');
                    if (elsoX < szelsoErtekek[2])
                        szelsoErtekek[2] = elsoX;
                    //maxM
                    utolsoX = fajl[i].LastIndexOf('x');
                    if (utolsoX > szelsoErtekek[3])
                        szelsoErtekek[3] = utolsoX;
                    eddigNincsFekete = false;
                }
            }

            nincsFekete = eddigNincsFekete;
        }

        private Int32 initFeketekSzama()
        {
            Int32 szam = 0;
            foreach (String sor in matrix)
            {
                szam += Regex.Matches(sor, "x").Count;
            }
            return szam;
        }

        private Boolean konzisztensAFolt()
        {
            int i = 0;
            int j = 0;
            Boolean elso = true;

            holAzElsoFekete();

            if (elsoFekete.Equals(new int[2] { 0, 0 }))
            {
                nincsFekete = true;
            }

            // Egytol indexelek, mert a nulladik sorban es oszlopban nincs relevans adat 
            // ezenkivul 8-ig mert sem az utolso sorban sem az utolso oszlopban nincsen relevans adat a foltra vonatkozoan
            for (i = 1; i < dimenziok[0] - 1; i++)
            {
                for (j = 1; j < dimenziok[1] - 1; j++)
                {
                    if (matrix[i][j] == 'x')
                    {
                        if (!egyFeketeFoltVan)
                        {
                            return false;
                        }
                        egyFeketeFoltVan = allomanyKonzisztensAdottPontbol(matrix, "fekete", i, j, elso);
                        elso = false;
                    }
                }
            }

            return true;
        }

        private void holAzElsoFekete()
        {
            elsoFekete = new int[2] { 0, 0 };

            // 1-tol kezdjuk az indexelest, mivel az elso oszlopban nem lehet fekete
            for (int i = 1; i < dimenziok[0]; i++)
            {
                int xIndex = matrix[i].IndexOf("x");
                if (xIndex != -1)
                {
                    elsoFekete[0] = i;
                    elsoFekete[1] = xIndex;
                    break;
                }
            }
        }

        // Hogyha egy adott feher ponthoz tartozo feher allomanyt nem vesz korul konzisztens fekete mezo, akkor 
        // ez egy szabad terulet, nem a fekete allomany belsejeben van.
        // Ellenkezo esetben a feher mezo a fekete allomany belsejeben van, tehat a bemeneti fajl hibas
        private Boolean vanFeherFolt()
        {
            int i = 0;
            int j = 0;
            Boolean elso = true;

            // Egytol indexelek, mert a nulladik sorban es oszlopban nincs relevans adat 
            // ezenkivul 8-ig mert sem az utolso sorban sem az utolso oszlopban nincsen relevans adat a foltra vonatkozoan
            for (i = 1; i < dimenziok[0] - 1; i++)
            {
                for (j = 1; j < dimenziok[1] - 1; j++)
                { 
                    if (matrix[i][j] == '.')
                    {
                        allomanyKonzisztensAdottPontbol(matrix, "feher", i, j, elso);
                        elso = false;
                    }
                }
            }

            foreach (int[,] elem in voltMarFeher)
            {
                //Nincsen kiut
                if (elem[0, 0] != -2)
                    return true;
            }

            return false;
        }

        // Igaz, ha az allomanyban talal olyan elemet, ami korabban mar szerepelt, mert ekkor egy osszefuggo halmazrol beszelhetunk
        // Hamis, ha nem talal olyan elemet, amelyik korabban szerepelt, mert ekkor egy uj, diszjunkt halmazrol beszelunk
        private Boolean allomanyKonzisztensAdottPontbol(string[] tartomany, string szin, int iTol, int jTol, Boolean elso)
        {
            int[,] elozoHelyek;
            Boolean kiertem = false;

            if (szin.Equals("fekete"))
            {
                elozoHelyek = BufferKezeles.initBuffer(leghosszabbFeketeUt);
            }
            else
                elozoHelyek = BufferKezeles.initBuffer((dimenziok[0] - 2) * (dimenziok[1] - 2));

            kiertem = korbejaras(tartomany, szin, iTol, jTol, ref elozoHelyek);

            BufferKezeles.normalizal(ref elozoHelyek);

            // Elozoleg meglatogatott helyek hozzaadasa a kijelolt halmazhoz
            for (int i = 0; i<elozoHelyek.Length/2; i++)
            {
                int[] lepes;
                if (elozoHelyek[i, 0] != -1 && elozoHelyek[i, 1] != -1)
                     lepes = new int[2] { elozoHelyek[i, 0], elozoHelyek[i, 1] };
                else continue;

                if (szin.Equals("fekete"))
                {
                    if(elso || BufferKezeles.tartalmazza(voltMarFekete, lepes))
                    {
                        BufferKezeles.egyediElemekTombjeHozzaadas(ref voltMarFekete, elozoHelyek);
                        return true;
                    }
                }
                else
                {
                    foreach (int[,] elem in voltMarFeher)
                    {
                        if(elso || BufferKezeles.tartalmazza(elem,lepes))
                        {
                            int index = voltMarFeher.IndexOf(elem);
                            int[,] halmaz = elem;
                            BufferKezeles.egyediElemekTombjeHozzaadas(ref halmaz, elozoHelyek);
                            if (kiertem)
                            {
                                halmaz[0, 0] = -2;
                                halmaz[0, 1] = -2;
                            }
                            voltMarFeher[index] = halmaz;
                            return true;
                        }
                    }
                    int[,] ujHalmaz = BufferKezeles.initBuffer(elozoHelyek.Length / 2);
                    BufferKezeles.egyediElemekTombjeHozzaadas(ref ujHalmaz, elozoHelyek);
                    if (kiertem)
                    {
                        ujHalmaz[0, 0] = -2;
                        ujHalmaz[0, 1] = -2;
                    }
                    int[,] element;
                    try {
                        element = voltMarFeher.Last();
                        voltMarFeher.Insert(voltMarFeher.IndexOf(element) + 1, ujHalmaz);
                    }
                    catch(InvalidOperationException e)
                    {
                        voltMarFeher.Insert(0, ujHalmaz);
                    }
                    
                    if(!kiertem)
                        return true;
                }
            }
            
            return false;
        }

        private Boolean korbejaras(string[] tartomany, string szin, int iTol, int jTol, ref int[,] elozoHelyek)
        {
            int[] kovHely = new int[2] { iTol, jTol };
            int[,] visszafordulas = BufferKezeles.initBuffer(3);
            int elozoLepesIranya = 2;

            do
            {
                if (szin.Equals("fekete"))
                    kovHely = feketeLepes(tartomany, kovHely, ref elozoLepesIranya, visszafordulas);
                else
                {
                    kovHely = feherLepes(kovHely, ref elozoLepesIranya);
                    //Ha kiert a szelere
                    if (kovHely[0] == szelsoErtekek[0] || kovHely[0] == szelsoErtekek[1]
                        || kovHely[1] == szelsoErtekek[2] || kovHely[1] == szelsoErtekek[3])
                    {
                        return true;
                    }
                }

                if (BufferKezeles.ketszerTartalmazza(elozoHelyek, kovHely))
                    return false;

                BufferKezeles.shiftBuffer(ref elozoHelyek, kovHely);
                BufferKezeles.shiftBuffer(ref visszafordulas, kovHely);
            } while (!(kovHely[0] == elsoFekete[0] && kovHely[1] == elsoFekete[1]));

            // Erdektelen
            return false;
        }

        private int[] feketeLepes(string[] tartomany, int[] kovHely, ref int elozoLepes, int[,] visszafordulas)
        {
            int[] lepesTemp = new int[2] { -1, -1 };
            int ellenorzesekSzama = 0;
            char jel = 'x';

            // Ne menjen vissza, se egyel tovabbi iranyba (mert ezt megtehette volna elobb is)
            // Ha mind a het ellenorzesen atment, akkor van vege a ciklusnak
            int kovIranyTemp = (elozoLepes - 2) % 8;
            if (kovIranyTemp < 0)
                kovIranyTemp += 8;
            switch (kovIranyTemp)
            {
                case 0:
                    lepesTemp[0] = kovHely[0] - 1;
                    lepesTemp[1] = kovHely[1];
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 0;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 1;
                case 1:
                    lepesTemp[0] = kovHely[0] - 1;
                    lepesTemp[1] = kovHely[1] + 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel)
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 1;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 2;
                case 2:
                    lepesTemp[0] = kovHely[0];
                    lepesTemp[1] = kovHely[1] + 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 2;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 3;
                case 3:
                    lepesTemp[0] = kovHely[0] + 1;
                    lepesTemp[1] = kovHely[1] + 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 3;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 4;
                case 4:
                    lepesTemp[0] = kovHely[0] + 1;
                    lepesTemp[1] = kovHely[1];
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 4;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 5;
                case 5:
                    lepesTemp[0] = kovHely[0] + 1;
                    lepesTemp[1] = kovHely[1] - 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 5;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 6;
                case 6:
                    lepesTemp[0] = kovHely[0];
                    lepesTemp[1] = kovHely[1] - 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 6;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 7;
                case 7:
                    lepesTemp[0] = kovHely[0] - 1;
                    lepesTemp[1] = kovHely[1] - 1;
                    if (tartomany[lepesTemp[0]][lepesTemp[1]] == jel )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 7;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 8;
                // Ha mar vegigfutott az osszes ellenorzesen, akkor legyen vege 
                case 8:
                    if (ellenorzesekSzama > 7)
                        break;
                    else
                        goto case 0;
            }
            
            return kovHely;
        }

        private int[] feherLepes(int[] kovHely, ref int elozoLepes)
        {
            int ellenorzesekSzama = 0;
            int[] lepesTemp = new int[2] { -1, -1 };

            int kovIranyTemp = (elozoLepes - 2) % 8;
            if (kovIranyTemp < 0)
                kovIranyTemp += 8;

            // Ne menjen vissza, se egyel tovabbi iranyba (mert ezt megtehette volna elobb is)
            // Ha mind a het ellenorzesen atment, akkor van vege a ciklusnak
            switch (kovIranyTemp)
            {
                case 0:
                    lepesTemp[0] = kovHely[0] - 1;
                    lepesTemp[1] = kovHely[1];
                    if (matrix[lepesTemp[0]][lepesTemp[1]] == '.' )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 0;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 2;
                case 2:
                    lepesTemp[0] = kovHely[0];
                    lepesTemp[1] = kovHely[1] + 1;
                    if (matrix[lepesTemp[0]][lepesTemp[1]] == '.' )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 2;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 4;
                case 4:
                    lepesTemp[0] = kovHely[0] + 1;
                    lepesTemp[1] = kovHely[1];
                    if (matrix[lepesTemp[0]][lepesTemp[1]] == '.' )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 4;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 6;
                case 6:
                    lepesTemp[0] = kovHely[0];
                    lepesTemp[1] = kovHely[1] - 1;
                    if (matrix[lepesTemp[0]][lepesTemp[1]] == '.' )
                    {
                        kovHely = lepesTemp;
                        elozoLepes = 6;
                        break;
                    }
                    ellenorzesekSzama++;
                    goto case 8;
                case 8:
                    if (ellenorzesekSzama > 4)
                        break;
                    else
                        goto case 0;
            }            

            return kovHely;
        }

        public void fajlKiiras(int sorszam)
        {
            if (nincsFekete)
            {
                kimenet = new String[] { "NINCS FOLT" };
            }
            else
            {
                kimenetAlkotas();
            }

            System.IO.File.WriteAllLines(Environment.CurrentDirectory + "\\fajlok\\FOLT" + sorszam + ".KI.txt", kimenet);
        }

        private void kimenetAlkotas()
        {
            kimenet = new string[dimenziok[0]];
            kimenet[0] = (elsoFekete[0] + 1) + " " + (elsoFekete[1] + 1);
            int[,] korbejarasUtja = BufferKezeles.initBuffer(leghosszabbFeketeUt);
            korbejaras(matrix, "fekete", elsoFekete[0], elsoFekete[1], ref korbejarasUtja);

            string lanckod = "";
            int i = 0;
                        
            while (i < leghosszabbFeketeUt - 1)
            {
                int elsoParam;
                int masodikParam;

                if (korbejarasUtja[i, 0] == -1 && korbejarasUtja[i + 1, 0] == -1)
                {
                    i++;
                    continue;
                }
                else if (korbejarasUtja[i, 0] == -1)
                {
                    elsoParam = korbejarasUtja[i + 1, 0] - elsoFekete[0];
                    masodikParam = korbejarasUtja[i + 1, 1] - elsoFekete[1];
                }
                else
                {
                    elsoParam = korbejarasUtja[i + 1, 0] - korbejarasUtja[i, 0];
                    masodikParam = korbejarasUtja[i + 1, 1] - korbejarasUtja[i, 1];
                }
                string irany =  elsoParam + " - " + masodikParam;

                switch (irany)
                {
                    case "-1 - 0":
                        lanckod += 1;
                        break;
                    case "-1 - -1":
                        lanckod += 2;
                        break;
                    case "0 - -1":
                        lanckod += 3;
                        break;
                    case "1 - -1":
                        lanckod += 4;
                        break;
                    case "1 - 0":
                        lanckod += 5;
                        break;
                    case "1 - 1":
                        lanckod += 6;
                        break;
                    case "0 - 1":
                        lanckod += 7;
                        break;
                    case "-1 - 1":
                        lanckod += 8;
                        break;
                    default:
                        break;
                }

                i++;
            }

            kimenet[1] = lanckod;
            Console.WriteLine("A mátrix lánckódja: " + lanckod);
        }
    }
}

