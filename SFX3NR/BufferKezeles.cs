using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFX3NR
{
    class BufferKezeles
    {
        public static int[,] initBuffer(int hossz)
        {
            int[,] buffer = new int[hossz, 2];

            for (int i = 0; i < hossz; i++)
            {
                buffer[i, 0] = -1;
                buffer[i, 1] = -1;
            }

            return buffer;
        }

        public static void shiftBuffer(ref int[,] buffer, int[] lepes)
        {
            for (int it = 0; it < (buffer.Length / 2) - 1; it++)
            {
                buffer[it, 0] = buffer[it + 1, 0];
                buffer[it, 1] = buffer[it + 1, 1];
            }

            buffer[(buffer.Length / 2) - 1, 0] = lepes[0];
            buffer[(buffer.Length / 2) - 1, 1] = lepes[1];
        }

        public static Boolean tartalmazza(int[,] buffer, int[] lepes)
        {
            for (int it = 0; it < buffer.Length / 2; it++)
            {
                if (buffer[it, 0] == lepes[0] && buffer[it, 1] == lepes[1])
                    return true;
            }
            return false;
        }

        public static Boolean ketszerTartalmazza(int[,] buffer, int[] lepes)
        {
            Boolean tartalmazza = false;

            for (int it = 0; it < buffer.Length / 2; it++)
            {
                if (tartalmazza && buffer[it, 0] == lepes[0] && buffer[it, 1] == lepes[1])
                    return true;
                else if (buffer[it, 0] == lepes[0] && buffer[it, 1] == lepes[1])
                    tartalmazza = true;
            }

            return false;
        }

        public static void normalizal(ref int[,] buffer)
        {
            int[,] bufferTemp = initBuffer(buffer.Length / 2);

            int j = (bufferTemp.Length / 2) - 1;
            for(int i = (buffer.Length / 2) - 1; i>=0; i--)
            {
                int[] lepes = new int[2] { buffer[i, 0], buffer[i, 1] };
                if(!tartalmazza(bufferTemp,lepes))
                {
                    bufferTemp[j, 0] = lepes[0];
                    bufferTemp[j, 1] = lepes[1];
                    j--;
                }
            }

            mergeSort(ref bufferTemp);

            buffer = bufferTemp;
        }

        private static Boolean mergeSort(ref int[,] buffer)
        {
            if (buffer.Length/2 == 3)
            {
                int[,] bufferTemp = new int[2, 2] { { buffer[0, 0], buffer[0, 1] }, { buffer[1, 0], buffer[1, 1] } };
                rendez(ref bufferTemp);
                beilleszt(ref buffer, bufferTemp, 0, 1);
                bufferTemp = new int[2, 2] { { buffer[1, 0], buffer[1, 1] }, { buffer[2, 0], buffer[2, 1] } };
                rendez(ref bufferTemp);
                beilleszt(ref buffer, bufferTemp, 1, 2);
                bufferTemp = new int[2, 2] { { buffer[0, 0], buffer[0, 1] }, { buffer[1, 0], buffer[1, 1] } };
                rendez(ref bufferTemp);
                beilleszt(ref buffer, bufferTemp, 0, 1);
                return true;
            }
            else if (buffer.Length/2 == 2)
            {
                rendez(ref buffer);
                return true;
            }

            double elemszam = buffer.Length / 4;
            int felIndex = (int)Math.Ceiling(elemszam);

            int[,] balFa = initBuffer(felIndex);
            for (int i = felIndex - 1; i >= 0; i--)
            {
                balFa[i, 0] = buffer[i, 0];
                balFa[i, 1] = buffer[i, 1];
            }

            int[,] jobbFa = initBuffer((buffer.Length / 2) - felIndex);
            for(int i = buffer.Length / 2 - 1; i>=felIndex; i--)
            {
                jobbFa[i - felIndex, 0] = buffer[i, 0];
                jobbFa[i - felIndex, 1] = buffer[i, 1];
            }

            int[] lepeske = new int[2] { 1, 3 };
            if (tartalmazza(balFa, lepeske))
                ;
            else if (tartalmazza(jobbFa, lepeske))
                ;

            if (mergeSort(ref balFa))
                mergeSort(ref jobbFa);

            osszefesules(ref buffer, balFa, jobbFa);
            
            return true;
        }

        private static void rendez(ref int[,] buffer)
        {
            if (buffer[0, 0] > buffer[1, 0])
            {
                helycsere(ref buffer, 0, 1);

            }
            else if (buffer[0, 0] == buffer[1, 0] && buffer[0, 1] > buffer[1, 1])
            {
                helycsere(ref buffer, 0, 1);
            }
        }

        private static void beilleszt(ref int[,] buffer, int[,] bufferTemp, int index0, int index1)
        {
            buffer[index0, 0] = bufferTemp[0, 0];
            buffer[index0, 1] = bufferTemp[0, 1];
            buffer[index1, 0] = bufferTemp[1, 0];
            buffer[index1, 1] = bufferTemp[1, 1];
        }

        private static void osszefesules(ref int[,] buffer, int[,] balFa, int[,] jobbFa)
        {
            int i = 0;
            int j = 0;

            for (int k = 0; k < buffer.Length / 2;)
            {
                if (i < balFa.Length / 2  && j < jobbFa.Length / 2 && balFa[i, 0] < jobbFa[j, 0])
                {
                    buffer[k, 0] = balFa[i, 0];
                    buffer[k, 1] = balFa[i, 1];
                    i++;
                    k++;
                }
                else if (i < balFa.Length / 2 && j < jobbFa.Length / 2 && balFa[i, 0] == jobbFa[j, 0] && balFa[i, 1] < jobbFa[j, 1])
                {
                    buffer[k, 0] = balFa[i, 0];
                    buffer[k, 1] = balFa[i, 1];
                    i++;
                    k++;
                }
                else if (i < balFa.Length / 2 && j < jobbFa.Length / 2)
                {
                    buffer[k, 0] = jobbFa[j, 0];
                    buffer[k, 1] = jobbFa[j, 1];
                    j++;
                    k++;
                }
                else if (j < jobbFa.Length / 2)
                {
                    buffer[k, 0] = jobbFa[j, 0];
                    buffer[k, 1] = jobbFa[j, 1];
                    j++;
                    k++;
                }
                else if (i < balFa.Length / 2)
                {
                    buffer[k, 0] = balFa[i, 0];
                    buffer[k, 1] = balFa[i, 1];
                    i++;
                    k++;
                }
                else
                    break;
            }
        }

        private static void helycsere(ref int[,] buffer, int i, int j)
        {
            int[] temp = new int[2] { buffer[i, 0], buffer[i, 1] };
            buffer[i, 0] = buffer[j, 0];
            buffer[i, 1] = buffer[j, 1];
            buffer[j, 0] = temp[0];
            buffer[j, 1] = temp[1];
        }

        public static void egyediElemekTombjeHozzaadas(ref int[,] buffer, int[,] lepesek)
        {
            for(int i = (lepesek.Length / 2) - 1; i>=0; i--)
            {
                int[] lepes = new int[2] { lepesek[i, 0], lepesek[i, 1] };
                if (!tartalmazza(buffer, lepes))
                {
                    for (int j = (buffer.Length / 2) - 1; j >= 0; j--)
                    {
                        if (buffer[j, 0] == -1 && buffer[j, 1] == -1)
                        {
                            buffer[j, 0] = lepes[0];
                            buffer[j, 1] = lepes[1];
                            break;
                        }
                    }
                }
            }
        }
    }
}
