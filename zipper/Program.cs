using System;
using System.IO;
using System.Collections.Generic;

namespace zipper
{


    /// <summary>
    /// byte  vs 8 bits
    /// 
    /// 10d   -->  1010b
    /// 
    /// 00001100b -->  0*1 + 0*2 + 1*4 +1*8=12d  --> 1100b
    /// 
    /// 255d ==> 11111111b
    /// </summary>
    class Program
    {
        /// <summary>
        /// zip a file
        /// </summary>
        /// <algo>make
        /// fixed bitlength pro byte  ==> variable bitlength pro byte
        /// </algo>
        static void Main(string[] args)
        {
            byte[] file = File.ReadAllBytes("abc.txt");
            int[] cnt_freq = funcs.countFreq(file);
            lijst nodelist = new lijst();
            node head = funcs.make_dll(cnt_freq, nodelist);
            node topoftree = funcs.make_tree(head, nodelist);
            string[] tabel_bitcodes = funcs.make_bitcodes(topoftree);
            string encoded_file_as_string = funcs.translate(file, tabel_bitcodes);
            byte[] encoded_file_as_bytes = funcs.convert_string_2_bytes(encoded_file_as_string); //xtrbits
            string encoded_tree_as_string = funcs.save_tree(topoftree);
            // byte[] encoded_tree_as_bytes = funcs.convert_string_2_bytes(encoded_tree_as_string);
        }

        class funcs
        {
            /// <summary>
            /// return freq of each byte
            /// </summary>
            /// <algo>
            /// 000: 0
            /// 001: 0
            /// ....
            /// 097: 3
            /// 098: 3
            /// 099: 2
            /// 100: 1
            /// ...
            /// use bytevalue as index of count
            /// for example:  file: 231 23 123 4 23
            /// </algo>
            internal static int[] countFreq(byte[] file)
            {
                int[] count = new int[256];
                for (int i = 0; i < file.Length; i++)
                {
                    count[file[i]]++;
                }
                return count;
            }

            /// <summary>
            /// make new non-fixed-length bitcode for each byte
            /// </summary>
            /// <algo>
            /// L 1  , R 0
            /// 
            ///                            (0,9)
            ///                         (97,3)(0,6)
            ///                             (0,3) (98,3)
            ///                         (100,1)(99,2)
            ///                       
            ///  97: "1";
            ///  98: "00";
            ///  99: "010";
            /// 100: "011"; 
            /// 
            /// </algo>
            internal static string[] make_bitcodes(node topoftree)
            {
                string[] bitcodes = new string[256];
                recurse_down(topoftree, bitcodes, "");
                return bitcodes;
            }

            /// <summary>
            /// add 
            /// </summary>
            /// <algo>
            /// start at topoftree and walk all paths 
            /// 
            /// Leaf        : save path in tabel_bitcodes
            /// non_Leaf    : go L and save extra '1',  go R and save extra '0'
            /// 
            /// </algo>
            private static void recurse_down(node current, string[] bitcodes, string s)
            {
                if (current.L == null)
                {
                    bitcodes[current.b] = s;
                }
                else
                {
                    recurse_down(current.L, bitcodes, s + "1");
                    recurse_down(current.R, bitcodes, s + "0");

                }
            }


            /// <summary>
            ///  return head of a dll asc sorted on freq
            /// </summary>
            /// <algo>
            /// head (100,1) <==> (99,2)  <==> (98,3) <==> (97,3) (is the tail)
            /// </algo>
            internal static node make_dll(int[] cnt_freq, lijst nl)
            {
                for (int i = 0; i < cnt_freq.Length; i++)
                {
                    if (cnt_freq[i] != 0)
                    {
                        node n = new node((byte)i, cnt_freq[i]);
                        nl.add_node_ordered(n);
                    }
                }
                return nl.head;
            }



            /// <summary>
            ///  return top of tree (in order to get variable bitlength codes for each byte)
            /// </summary>
            /// <algo>
            ///  Huffmann
            ///  =========
            ///  
            ///  define a node as [b,f,P,N,L,R]
            ///
            ///  make new node and combine freq of head and head.N
            ///  non-leaf   [0,3,0,0,l,r]
            ///  [100,1,p,n,0,0] <-> [99,2,p,n,0,0]  <-> [98,3,p,n,0,0] <-> [97,3,p,n,0,0]
            ///   
            ///  add_asc_order 2 dll the node of line 62
            ///  shift head 2 times to the next
            ///  
            ///  repeat this until there is only one node left (head.next==null)
            ///  
            ///  97  ==> 01100001 
            ///
            ///  huffman: 
            ///  make non-leaf n that combines freq of 2 least frequent bytes
            ///  add that node to the dll
            ///  c=c.nxt.nxt;     
            ///  
            ///  convention of me:  L is a 1, to R is a 0
            /// 
            /// 
            /// </algo>
            internal static node make_tree(node head, lijst nl)
            {
                node c = head;
                while (c.nxt != null)
                {
                    node n = new node(0, c.f + c.nxt.f);
                    n.L = c;
                    n.R = c.nxt;
                    nl.add_node_ordered(n);
                    c = c.nxt.nxt;
                }
                return c;
            }

            /// <summary>
            /// return translated file as string 
            /// </summary>
            /// <algo>
            /// file 97 98 99 97 98 99 97 98 100
            /// 97  -> '1'
            /// string s="100010100010100011";
            /// </algo>
            internal static string translate(byte[] file, string[] tabel_bitcodes)
            {
                string s = "";
                for (int i = 0; i < file.Length; i++)
                {
                    s += tabel_bitcodes[file[i]];
                }
                return s;
            }

            /// <summary>
            /// return corresponding byte[]
            /// </summary>
            /// <algo>
            /// string s="10001010 - 00101000  - 11 000000"; ==>  byte[] = 138,40,192
            /// add a byte to represent the extra added bits i.c. 6
            /// 
            /// byte[] bar = {138,40,192,6};
            /// </algo>
            internal static byte[] convert_string_2_bytes(string s)
            {
                byte rest = (byte)(8 - s.Length % 8);

                while (s.Length % 8 != 0)
                {
                    s += "0";
                }
                byte[] b = new byte[s.Length / 8 + 1];
                string temp;
                b[b.Length - 1] = rest;
                for (int i = 0; i < b.Length - 1; i++)
                {
                    temp = "";
                    for (int j = i * 8; j < i * 8 + 8; j++)
                    {
                        temp += s[j];
                    }
                    b[i] = BitsToByte(temp);
                }
                return b;
            }

            private static byte BitsToByte(string temp)
            {
                byte b = 0;
                byte start = 128;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] == '1')
                    {
                        b += start;
                    }
                    start /= 2;
                }
                return b;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <algo>
            ///                            (0,9)
            ///                         (97,3)(0,6)
            ///                             (0,3) (98,3)
            ///                         (100,1)(99,2)
            /// 
            ///  our convention
            ///  NL:  0   L,R
            ///  L :  1   save relevant data as  (8) bits
            ///  
            ///  s="01[97]001[100]1[99]1[98]";
            ///  s="01[97]001[100]1[99]1[98]";
            /// 
            ///                            (0,0)
            ///                         (97,0)
            /// 
            /// </algo>
            internal static string save_tree(node topoftree)
            {
                piet = "";
                recurse_save_down(topoftree);
                return piet;
            }

            static string piet = "";


            private static void recurse_save_down(node current)
            {
                if (current.L == null)
                {
                    piet += "1";
                    piet += byte2_8bits(current.b);
                }
                else
                {
                    piet += "0";
                    recurse_save_down(current.L);
                    recurse_save_down(current.R);
                }
            }

            private static string byte2_8bits(byte b)
            {
                return b;
            }
        }
    }

    class node
    {
        public byte b;
        public int f;
        public node prev, nxt, L, R;
        public node(byte b, int f)
        {
            this.b = b;
            this.f = f;
        }
    }

    class lijst
    {
        public node head, tail;

        /// <summary>
        /// make a node with f and b 
        /// and add that node in such a wat that the list stays ordered asc
        /// </summary>
        internal node add2orderedlist(int f, byte b)
        {
            node n = new node(b, f);
            add_node_ordered(n);
            return head;
        }

        /// <summary>
        /// Add  node n in such a way that the list stays ordered asc
        /// </summary>
        internal void add_node_ordered(node n)
        {
            if (head == null)
            {
                head = n; tail = n;
            }
            else
            {
                if (n.f <= head.f)
                {
                    head.prev = n;
                    n.nxt = head;
                    head = n;
                }
                else
                {
                    if (n.f > tail.f)
                    {
                        tail.nxt = n;
                        n.prev = tail;
                        tail = n;
                    }
                    else
                    {
                        node after = head.nxt;
                        while (n.f > after.f)
                        {
                            after = after.nxt;
                        }
                        node before = after.prev;
                        before.nxt = n;
                        after.prev = n;
                        n.prev = before;
                        n.nxt = after;
                    }
                }
            }
        }
    }
}

