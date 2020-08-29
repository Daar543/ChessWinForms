﻿namespace Sachy_Obrazky
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Collections;
    using System.Runtime.InteropServices;

    public partial class Engine
    {
        public const int
            Rank_1 = 7,
            Rank_2 = 6,
            Rank_3 = 5,
            Rank_4 = 4,
            Rank_5 = 3,
            Rank_6 = 2,
            Rank_7 = 1,
            Rank_8 = 0,
            File_A = 0,
            File_B = 1,
            File_C = 2,
            File_D = 3,
            File_E = 4,
            File_F = 5,
            File_G = 6,
            File_H = 7;
        public const byte
            pawn = 1,
            knight = 2,
            bishop = 3,
            rook = 4,
            queen = 5,
            king = 6;
        public const uint emptymove = ((uint)1 << 27) - 1;
        public static Dictionary<char, byte> Types = new Dictionary<char, byte>
        {
            {'K',0 },{'P',1},{'N',2},{'B',3},{'R',4},{'Q',5 },
            {'k',6 },{'p',7},{'n',8},{'b',9},{'r',10},{'q',11 },
        };
        public static Dictionary<char, short> Values_Char = new Dictionary<char, short>
        {
            {'K',short.MaxValue },{'P',100},{'N',320},{'B',330},{'R',500},{'Q',900 },
            {'k',short.MaxValue },{'p',100},{'n',320},{'b',330},{'r',500},{'q',900 },
        };
        public static readonly int[] Values = new int[]
        {
            short.MaxValue, 100,320,330,500,900,
            short.MaxValue, 100,320,330,500,900,
        };
        public static Random los = new Random();

        public static readonly int[][] Posit_Values = new int[][]
        {
            //king early/mid
            new int[]
            {
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -20,-30,-30,-40,-40,-30,-30,-20,
                -10,-20,-20,-20,-20,-20,-20,-10,
                 20, 20,  0,  0,  0,  0, 20, 20,
                 20, 30, 10,  0,  0, 10, 30, 20
            },
            //pawn
            new int[]
            {
                0,  0,  0,  0,  0,  0,  0,  0,
                50, 50, 50, 50, 50, 50, 50, 50,
                10, 10, 20, 30, 30, 20, 10, 10,
                 0,  5, 10, 15, 15, 10,  5,  0,
               -10,  0,  5,  5,  5,  5,  0,-10,
                -5, -5,-10,  0,  0,-10, -5, -5,
                -5, 10, 10,  0,  0, 10, 10, -5,
                 0,  0,  0,  0,  0,  0,  0,  0
            },
            //knight
            new int[]
            {
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -20,  5, 15, 20, 20, 15,  5,-20,
                -20,  0, 15, 20, 20, 15,  0,-20,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50,
            },
            //bishosh
            new int[]
            {
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0, 15, 10, 10, 15,  0,-10,
                -10, 15, 10, 10, 10, 10, 15,-10,
                -10, 15,  5,  0,  0,  5, 15,-10,
                -20,-10,-10,-10,-10,-10,-10,-20,
            },
            //rook
            new int[]
            {
                0,  0,  0,  0,  0,  0,  0,  0,
                5, 10, 10, 10, 10, 10, 10,  5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                0,  0,  0,  5,  5,  5,  0,  0
            },
            //queen
            new int[]
            {
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5,  5,  5,  5,  0,-10,
                 -5,  0,  5,  5,  5,  5,  0, -5,
                  0,  0,  5,  5,  5,  5,  0,  0,
                -10,  5,  5,  5,  5,  5,  5,-10,
                -10,  0,  5,  0,  0,  5,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20
            },
            //king late
            new int[]
            {
                -50,-40,-30,-20,-20,-30,-40,-50,
                -30,-20,-10,  0,  0,-10,-20,-30,
                -30,-10, 20, 30, 30, 20,-10,-30,
                -30,-10, 30, 40, 40, 30,-10,-30,
                -30,-10, 30, 40, 40, 30,-10,-30,
                -30,-10, 20, 30, 30, 20,-10,-30,
                -30,-30,  0,  0,  0,  0,-30,-30,
                -50,-30,-30,-30,-30,-30,-30,-50
            },
        };

        const ulong blacksquares = 0xAA55AA55AA55AA55;
        const ulong whitesquares = ~0xAA55AA55AA55AA55;

        public static readonly uint[] Priorities = new uint[]
        {
            1,6,5,4,3,2,
            1,6,5,4,3,2,
        };
        public static readonly int[] Indices = new int[64]
        {
            0,1,2,3,4,5,6,7,
            16,17,18,19,20,21,22,23,
            32,33,34,35,36,37,38,39,
            48,49,50,51,52,53,54,55,
            64,65, 66,67,68,69,70,71,
            80, 81, 82,83,84,85,86, 87,
            96, 97,98, 99,100,101,102,103,
            112,113, 114, 115, 116,117, 118,119
        };
        public static int[] Deinds = new int[128]
        {
            0, 1, 2, 3, 4, 5, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1,
            8, 9, 10, 11, 12, 13, 14, 15,-1, -1, -1, -1, -1, -1, -1, -1,
            16, 17, 18, 19, 20, 21, 22, 23, -1, -1, -1, -1, -1, -1, -1, -1,
            24, 25, 26, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1, -1, -1, -1,
            32, 33, 34, 35, 36, 37, 38, 39, -1, -1, -1, -1, -1, -1, -1, -1,
            40, 41, 42, 43, 44, 45, 46, 47, -1, -1, -1, -1, -1, -1, -1, -1,
            48, 49, 50, 51, 52, 53, 54, 55, -1, -1, -1, -1, -1, -1, -1, -1,
            56, 57, 58, 59, 60, 61, 62, 63, -1, -1, -1, -1, -1, -1, -1, -1,
        };
        static readonly int[] Directions_King = new int[8] { -17, -16, -15, -1, +1, 15, 16, 17 };
        static readonly int[] Directions_Knight = new int[8] { -33, -31, -18, -14, +14, 18, 31, 33 };
        static readonly int[] Directions_Rook = new int[4] { -16, -1, 1, 16 };
        static readonly int[] Directions_Bishop = new int[4] { -17, -15, 15, 17 };
        static readonly int[] Directions_Queen = new int[8] { -17, -16, -15, -1, +1, 15, 16, 17 };
        static readonly int[] Directions_Pawn_White = new int[4] { -16, -32, -15, -17 };
        static readonly int[] Directions_Pawn_Black = new int[4] { 16, 32, 15, 17 };


        const int //bitboard indices
            BB_K = 0,
            BB_P = 1,
            BB_N = 2,
            BB_B = 3,
            BB_R = 4,
            BB_Q = 5,
            BB_k = 6,
            BB_p = 7,
            BB_n = 8,
            BB_b = 9,
            BB_r = 10,
            BB_q = 11;

        static ulong Wmask; //1 bit for every white piece on given square
        static ulong Bmask; //same, for black
        static ulong Block; //union
        public static int Position; // 1 repetition + blank moves, 2 moves, 3 moves+ep, 4 castling + color  
        public int GetPos()
        {
            return Position;
        }
            
        static int Wking;
        static int Bking;
        static ulong CurrentHash;
        public static Dictionary<uint, Hashentry>[] TranspoTable;
        public string Notation { get; set; }

        const int
            hash_side = 772,
            hash_castling = 768;


        static int HammingWeight(ulong x)
        {
            x = (x & 0x5555555555555555) + ((x >> 1) & 0x5555555555555555); //put count of each  2 bits into those  2 bits 
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333); //put count of each  4 bits into those  4 bits 
            x = (x & 0x0f0f0f0f0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f0f0f0f0f); //put count of each  8 bits into those  8 bits 
            x = (x & 0x00ff00ff00ff00ff) + ((x >> 8) & 0x00ff00ff00ff00ff); //put count of each 16 bits into those 16 bits 
            x = (x & 0x0000ffff0000ffff) + ((x >> 16) & 0x0000ffff0000ffff); //put count of each 32 bits into those 32 bits 
            x = (x & 0x00000000ffffffff) + ((x >> 32) & 0x00000000ffffffff); //put count of each 64 bits into those 64 bits 
            return (int)x;
        }
        //public static ulong[] Bitboards = new ulong[12];
        static ulong Land_Moves(int[] Directions, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = 0;
            int p;
            int di;
            foreach (int i in Directions)
            {
                p = Indices[index];
                p += i;
                if ((p & 0x88) != 0)
                    continue;
                di = Deinds[p];
                posmov |= (((ulong)1) << di);
            }
            ulong w = ~WhiteBoard;
            ulong b = ~BlackBoard;
            if (white)
                posmov &= w;
            else
                posmov &= b;
            return (posmov);
        }

        /*static ulong Land_Moves_Premade(ulong[] table, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        { //does not iterate through directions, just directly gets the possible moves from table (64 elements)
            ulong posmov = table[index];
            if (white)
                posmov &= (~WhiteBoard);
            else
                posmov &= (~BlackBoard);
            return posmov;
        }*/
        int[] AllPieces_White = new int[32];
        int[] AllPieces_Black = new int[32];
        static void Remask()
        {
            Wmask = BitBoards[0] | BitBoards[1] | BitBoards[2] | BitBoards[3] | BitBoards[4] | BitBoards[5];
            Bmask = BitBoards[6] | BitBoards[7] | BitBoards[8] | BitBoards[9] | BitBoards[10] | BitBoards[11];
            Block = Wmask | Bmask;
        }
        public static ulong Ray_Moves(int[] Directions, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = 0;
            int p;
            int di;
            //using 0x88 indices for this generation
            foreach (int i in Directions)
            {
                p = Indices[index];
                while (true) //moves into one direction until it falls off or crashes
                {
                    p += i;
                    if ((p & 0x88) != 0)
                        break;
                    di = Deinds[p];
                    posmov |= (((ulong)1) << di);
                    if (Bit(Block, di)) //if blocked, stop in this direction
                        break;
                }

            }
            if (white)
                posmov &= (~WhiteBoard);
            else
                posmov &= (~BlackBoard);
            return (posmov);
        }


        //only captures
        static ulong Land_Captures(int[] Directions, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            //move generation is completely the same, except it ignores empty squares
            ulong posmov = 0;
            int p;
            int di;
            foreach (int i in Directions)
            {
                p = Indices[index];
                p += i;
                if ((p & 0x88) != 0)
                    continue;
                di = Deinds[p];
                posmov |= (((ulong)1) << di);
            }
            ulong w = ~WhiteBoard;
            ulong b = ~BlackBoard;
            if (white)
            {
                posmov &= w;
                posmov &= BlackBoard;
            }

            else
            {
                posmov &= b;
                posmov &= WhiteBoard;
            }

            return (posmov);
        }

        static ulong Land_Captures_Premade(ulong[] table, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        { //does not iterate through directions, just directly gets the possible moves from table (64 elements)
            ulong posmov = table[index];
            if (white)
            {
                posmov &= (~WhiteBoard);
                posmov &= (BlackBoard);
            }

            else
            {
                posmov &= (~BlackBoard);
                posmov &= (WhiteBoard);
            }

            return posmov;
        }
        public static ulong Ray_Captures(int[] Directions, int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong poscaps = 0;
            int p;
            int di;
            //using 0x88 indices for this generation
            foreach (int i in Directions)
            {
                p = Indices[index];
                while (true) //moves into one direction until it falls off or crashes
                {
                    p += i;
                    if ((p & 0x88) != 0)
                        break;
                    di = Deinds[p];

                    if (Bit(Block, di)) //if blocked, stop in this direction
                    {
                        //and add it as capture, regardless of color
                        poscaps |= (((ulong)1) << di);
                        break;
                    }
                }

            }
            //now ignore pieces of the same color
            if (white)
                poscaps &= (~WhiteBoard);
            else
                poscaps &= (~BlackBoard);
            return (poscaps);
        }

        //KING
        static ulong King_Moves(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov1 = Land_Moves(Directions_King, index, WhiteBoard, BlackBoard, Block, white);
            //ulong posmov1 = Land_Moves_Premade(AllKingMoves, index, WhiteBoard, BlackBoard, Block, white);
            return posmov1;
        }
        static ulong FreeKingMoves(int index)
        {
            return King_Moves(index, 0, 0, 0, true);
        }
        //public static ulong[] AllKingMoves = new ulong[64];

        static ulong[] AllKingMoves = initKingMoves();

        static ulong[] initKingMoves()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeKingMoves(i);

            return result;
        }
        public static ulong MovesKing(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return white ? (AllKingMoves[index] & (~WhiteMask)) : (AllKingMoves[index] & (~BlackMask));
        }

        //KNIGHT

        static ulong Knight_Moves(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Land_Moves(Directions_Knight, index, WhiteBoard, BlackBoard, Block, white);
            return posmov;
        }
        static ulong FreeKnightMoves(int index)
        {
            return Knight_Moves(index, 0, 0, 0, true);
        }
        //public static ulong[] AllKnightMoves = new ulong[64];

        static ulong[] AllKnightMoves = initKnightMoves();

        static ulong[] initKnightMoves()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeKnightMoves(i);

            return result;
        }
        public static ulong MovesKnight(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return white ? (AllKnightMoves[index] & (~WhiteMask)) : (AllKnightMoves[index] & (~BlackMask));
        }

        //ROOK

        public static ulong Rook_Moves(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Moves(Directions_Rook, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeRookMoves(int index)
        {
            return Rook_Moves(index, 0, 0, 0, true);
        }
        //public static ulong[] AllRookMoves = new ulong[64];

        static ulong[] AllRookMoves = initRookMoves();

        static ulong[] initRookMoves()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeRookMoves(i);

            return result;
        }
        public static ulong MovesRook(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Rook_Moves(index, WhiteMask, BlackMask, Block, white);
        }

        //BISHOP

        public static ulong Bishop_Moves(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Moves(Directions_Bishop, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeBishopMoves(int index)
        {
            return Bishop_Moves(index, 0, 0, 0, true);
        }

        static ulong[] AllBishopMoves = initBishopMoves();

        static ulong[] initBishopMoves()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeBishopMoves(i);

            return result;
        }
        public static ulong MovesBishop(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Bishop_Moves(index, WhiteMask, BlackMask, Block, white);
        }

        //QUEEN

        public static ulong Queen_Moves(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Moves(Directions_Queen, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeQueenMoves(int index)
        {
            return Queen_Moves(index, 0, 0, 0, true);
        }

        static ulong[] AllQueenMoves = initQueenMoves();

        static ulong[] initQueenMoves()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeQueenMoves(i);

            return result;
        }
        public static ulong MovesQueen(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Queen_Moves(index, WhiteMask, BlackMask, Block, white);
        }

        //and now for captures

        //KING
        public static ulong CapturesKing(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            ulong caps = Land_Captures_Premade(AllKingMoves, index, WhiteMask, BlackMask, Block, white);
            return caps;
        }

        //KNIGHT
        public static ulong CapturesKnight(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            ulong caps = Land_Captures_Premade(AllKnightMoves, index, WhiteMask, BlackMask, Block, white);
            return caps;
        }

        //ROOK

        public static ulong Rook_Captures(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Captures(Directions_Rook, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeRookCaptures(int index)
        {
            return Rook_Captures(index, 0, 0, 0, true);
        }
        //public static ulong[] AllRookCaptures = new ulong[64];

        static ulong[] AllRookCaptures = initRookCaptures();

        static ulong[] initRookCaptures()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeRookCaptures(i);

            return result;
        }
        public static ulong CapturesRook(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Rook_Captures(index, WhiteMask, BlackMask, Block, white);
        }

        //BISHOP

        public static ulong Bishop_Captures(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Captures(Directions_Bishop, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeBishopCaptures(int index)
        {
            return Bishop_Captures(index, 0, 0, 0, true);
        }

        static ulong[] AllBishopCaptures = initBishopCaptures();

        static ulong[] initBishopCaptures()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeBishopCaptures(i);

            return result;
        }
        public static ulong CapturesBishop(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Bishop_Captures(index, WhiteMask, BlackMask, Block, white);
        }

        //QUEEN

        public static ulong Queen_Captures(int index, ulong WhiteBoard, ulong BlackBoard, ulong Block, bool white)
        {
            ulong posmov = Ray_Captures(Directions_Queen, index, WhiteBoard, BlackBoard, Block, white);
            return (posmov);
        }
        static ulong FreeQueenCaptures(int index)
        {
            return Queen_Captures(index, 0, 0, 0, true);
        }

        static ulong[] AllQueenCaptures = initQueenCaptures();

        static ulong[] initQueenCaptures()
        {
            int n = 64;
            var result = new ulong[n];

            for (int i = 0; i < n; ++i)
                result[i] = FreeQueenCaptures(i);

            return result;
        }
        public static ulong CapturesQueen(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            return Queen_Captures(index, WhiteMask, BlackMask, Block, white);
        }

        //PAWN
        public static ulong MovesPawn(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white)
        {
            int idx;
            int sq;
            int tar;
            ulong mo = 0;
            if (white)
            {
                idx = Indices[index];
                tar = idx + Directions_Pawn_White[0];
                if ((tar & 0x88) == 0) //not out of the edge
                {
                    sq = Deinds[tar];
                    if (!Bit(Block, sq)) //not blocked
                    {
                        mo |= (ulong)1 << sq;
                        if (48 <= index && index < 56) //double
                        {
                            tar = idx + Directions_Pawn_White[1];
                            if ((tar & 0x88) == 0)
                            {
                                sq = Deinds[tar];
                                if (!Bit(Block, sq))
                                    mo |= (ulong)1 << sq;
                            }
                        }
                    }
                }
                for (int i = 2; i <= 3; ++i)
                {
                    tar = idx + Directions_Pawn_White[i];
                    if ((tar & 0x88) != 0)
                        continue;
                    sq = Deinds[tar];
                    if (Bit(BlackMask, sq)) //if piece is for taking
                        mo |= (ulong)1 << sq;
                }
            }
            else
            {
                idx = Indices[index];
                tar = idx + Directions_Pawn_Black[0];
                if ((tar & 0x88) == 0) //not out of the edge
                {
                    sq = Deinds[tar];
                    if (!Bit(Block, sq)) //not blocked
                    {
                        mo |= (ulong)1 << sq;
                        if (8 <= index && index < 16) //double
                        {
                            tar = idx + Directions_Pawn_Black[1];
                            if ((tar & 0x88) == 0)
                            {
                                sq = Deinds[tar];
                                if (!Bit(Block, sq))
                                    mo |= (ulong)1 << sq;
                            }
                        }
                    }
                }
                for (int i = 2; i <= 3; ++i)
                {
                    tar = idx + Directions_Pawn_Black[i];
                    if ((tar & 0x88) != 0)
                        continue;
                    sq = Deinds[tar];
                    if (Bit(WhiteMask, sq)) //if piece is for taking
                        mo |= (ulong)1 << sq;
                }
            }

            return mo;
        }
        public static ulong[] initPawnCaps(bool white)
        {
            var captures = new ulong[64];
            for (int i = 0; i < captures.Length; ++i)
            {
                captures[i] = genCapturesPawn(i, white);
            }
            return captures;
        }

        public static short[] initEPawnCaps(bool white)
        {
            var captures = new short[64];
            for (int i = 0; i < captures.Length; ++i)
            {
                captures[i] = genECapturesPawn(i, white);
            }
            return captures;
        }
        public static ulong genCapturesPawn(int index, bool white)
        {
            //returns points which can be captured by pawn
            ulong mo = 0;
            int tar;
            int sq;
            int[] Dir;
            int idx = Indices[index];
            if (white)
                Dir = Directions_Pawn_White;
            else
                Dir = Directions_Pawn_Black;

            for (int i = 2; i <= 3; ++i)
            {
                tar = idx + Dir[i];
                if ((tar & 0x88) != 0)
                    continue;
                sq = Deinds[tar];
                mo |= (ulong)1 << sq;
            }
            return mo;
        }

        public static short genECapturesPawn(int index, bool white)
        {
            //returns two squares available for ep capture
            short mo = 0;
            int tar;
            int sq;
            int[] Dir;
            int idx = Indices[index];
            if (white)
                Dir = Directions_Pawn_White;
            else
                Dir = Directions_Pawn_Black;

            for (int i = 2; i <= 3; ++i)
            {
                tar = idx + Dir[i];
                if ((tar & 0x88) != 0)
                    continue;
                sq = Deinds[tar];
                mo |= (short)(sq << (8 * (3 - i)));
            }
            return mo;
        }

        public static readonly ulong[] CapturesPawn_White = initPawnCaps(true);
        public static readonly ulong[] CapturesPawn_Black = initPawnCaps(false);
        public static readonly short[] EP_Pawn_White = initEPawnCaps(true);
        public static readonly short[] EP_Pawn_Black = initEPawnCaps(false);
        static byte Castling_White(int position)
        {
            //least significant bits 1-4 signify castling, higher ones for white
            byte cas = (byte)((position >> 2) & 3);
            if ((cas & 1) != 0) //queenside
            {
                if (
                    ((Block & ((ulong)0b111 << 57)) != 0) || //if squares 57-59 are either bocked or attacked by opponent
                    (Attacked(60, Wmask, Bmask, Block, false, BitBoards) || Attacked(59, Wmask, Bmask, Block, false, BitBoards) || Attacked(58, Wmask, Bmask, Block, false, BitBoards))
                   )
                    cas ^= 1;
            }
            if ((cas & 2) != 0) //kingside
            {
                if (
                    ((Block & ((ulong)0b11 << 61)) != 0) || //if squares 61-62 are either bocked or attacked by opponent
                    (Attacked(60, Wmask, Bmask, Block, false, BitBoards) || Attacked(61, Wmask, Bmask, Block, false, BitBoards) || Attacked(62, Wmask, Bmask, Block, false, BitBoards))
                   )
                    cas ^= 2;
            }
            return cas;
        }

        static byte Castling_Black(int position)
        {
            //least significant bits 1-4 signify castling, lower ones for black
            byte cas = (byte)((position) & 3);
            if ((cas & 1) != 0) //queenside
            {
                if (
                    ((Block & (0b111 << 1)) != 0) || //if squares 1-3 are either bocked or attacked by opponent
                    (Attacked(4, Wmask, Bmask, Block, true, BitBoards) || Attacked(3, Wmask, Bmask, Block, true, BitBoards) || Attacked(2, Wmask, Bmask, Block, true, BitBoards))
                   )
                    cas ^= 1;
            }
            if ((cas & 2) != 0) //kingside
            {
                if (
                    ((Block & (0b11 << 5)) != 0) || //if squares 5-6 are either bocked or attacked by opponent
                    (Attacked(4, Wmask, Bmask, Block, true, BitBoards) || Attacked(5, Wmask, Bmask, Block, true, BitBoards) || Attacked(6, Wmask, Bmask, Block, true, BitBoards))
                   )
                    cas ^= 2;
            }
            return cas;
        }

        //SUPER PIECE - ATTACKED
        public static bool Attacked(int index, ulong WhiteMask, ulong BlackMask, ulong Block, bool white, ulong[] Bitboards) //last parameter is the attacking side!
        {
            //inverts colors
            if (white)
            {
                ulong pawns = CapturesPawn_Black[index];
                if ((Bitboards[BB_P] & pawns) != 0)
                    return true;

                ulong knights = CapturesKnight(index, BlackMask, WhiteMask, Block, true);
                if ((Bitboards[BB_N] & knights) != 0)
                    return true;

                ulong king = CapturesKing(index, BlackMask, WhiteMask, Block, true);
                if ((Bitboards[BB_K] & king) != 0)
                    return true;

                ulong bishopsQueens = CapturesBishop(index, BlackMask, WhiteMask, Block, true);
                if (((Bitboards[BB_B] | Bitboards[BB_Q]) & bishopsQueens) != 0)
                    return true;

                ulong rooksQueens = CapturesRook(index, BlackMask, WhiteMask, Block, true);
                if (((Bitboards[BB_R] | Bitboards[BB_Q]) & rooksQueens) != 0)
                    return true;

                return false;
            }
            else
            {
                ulong pawns = CapturesPawn_White[index];
                if ((Bitboards[BB_p] & pawns) != 0)
                    return true;

                ulong knights = CapturesKnight(index, BlackMask, WhiteMask, Block, false);
                if ((Bitboards[BB_n] & knights) != 0)
                    return true;

                ulong king = CapturesKing(index, BlackMask, WhiteMask, Block, false);
                if ((Bitboards[BB_k] & king) != 0)
                    return true;

                ulong bishopsQueens = CapturesBishop(index, BlackMask, WhiteMask, Block, false);
                if (((Bitboards[BB_b] | Bitboards[BB_q]) & bishopsQueens) != 0)
                    return true;

                ulong rooksQueens = CapturesRook(index, BlackMask, WhiteMask, Block, false);
                if (((Bitboards[BB_r] | Bitboards[BB_q]) & rooksQueens) != 0)
                    return true;

                return false;
            }
        }
        static ulong GenMoves(int index, int type, bool white, ulong WhiteMask, ulong BlackMask, ulong Block)
        {
            ulong moves = 0;
            switch (type)
            {
                case 0:
                case 6:
                    //moves = MovesCastling(index, Block, white);
                    moves |= MovesKing(index, WhiteMask, BlackMask, Block, white); break;
                case 1:
                case 7:
                    moves = MovesPawn(index, WhiteMask, BlackMask, Block, white); break;
                case 2:
                case 8:
                    moves = MovesKnight(index, WhiteMask, BlackMask, Block, white); break;
                case 3:
                case 9:
                    moves = MovesBishop(index, WhiteMask, BlackMask, Block, white); break;
                case 4:
                case 10:
                    moves = MovesRook(index, WhiteMask, BlackMask, Block, white); break;
                case 5:
                case 11:
                    moves = MovesQueen(index, WhiteMask, BlackMask, Block, white); break;

            }
            return moves;
        }

        static ulong GenCaptures(int index, int type, bool white, ulong WhiteMask, ulong BlackMask, ulong Block)
        {
            ulong captures = 0;
            switch (type)
            {
                case 0:
                case 6:
                    captures = CapturesKing(index, WhiteMask, BlackMask, Block, white); break;
                case 1:
                    captures = CapturesPawn_White[index];
                    captures &= BlackMask;
                    break;
                case 7:
                    captures = CapturesPawn_Black[index];
                    captures &= WhiteMask;
                    break;
                case 2:
                case 8:
                    captures = CapturesKnight(index, WhiteMask, BlackMask, Block, white); break;
                case 3:
                case 9:
                    captures = CapturesBishop(index, WhiteMask, BlackMask, Block, white); break;
                case 4:
                case 10:
                    captures = CapturesRook(index, WhiteMask, BlackMask, Block, white); break;
                case 5:
                case 11:
                    captures = CapturesQueen(index, WhiteMask, BlackMask, Block, white); break;

            }
            return captures;
        }

        static int[] EPGen(int epsquare, bool white)
        {
            var EP = new int[2];
            short b;
            if (white)
            {
                b = EP_Pawn_Black[epsquare];
                EP[0] = b & 0b11111111;
                EP[1] = (b >> 8) & 0b11111111;
                for (int i = 0; i <= 1; ++i)
                {
                    if (EP[i] != 0) //a8 cannot be ep-captured, therefore 0 will never appear if capture is possible
                    {
                        if (Bit(BitBoards[BB_P], EP[i])) //if there actually is white pawn
                        {
                            //builds move - see BMWhite
                            EP[i] <<= 16; //from
                            EP[i] |= (7 << 28) | (1 << 24) | (epsquare << 8) | (1 << 7) | (1);
                        }
                    }
                }
            }
            else
            {
                b = EP_Pawn_White[epsquare];
                EP[0] = b & 0b11111111;
                EP[1] = (b >> 8) & 0b11111111;
                for (int i = 0; i <= 1; ++i)
                {
                    if (EP[i] != 0) //a8 cannot be ep-captured, therefore 0 will never appear if capture is possible
                    {
                        if (Bit(BitBoards[BB_p], EP[i])) //if there actually is black pawn
                        {
                            //builds move - see BMBlack
                            EP[i] <<= 16; //from
                            EP[i] |= (1 << 28) | (7 << 24) | (epsquare << 8) | (1 << 7) | (1);
                        }
                    }
                }
            }
            return EP;
        }

        public int GetPiece(int idx)
        {
            int piece = -1;
            for(int i = 0; i < 12; ++i)
            {
                if (Bit(BitBoards[i], idx))
                {
                    piece = i;
                    break;
                }
            }
            return piece;
        }
        public static uint BuildMoveWhite(int piece, int from, int to)
        {
            /*var sw = new Stopwatch();
            sw.Start();*/
            //Warning: Does not cosider king capture
            //B1 = piece, B2 = From, B3 = To, B4 = Flags
            //Flags: 1b capture, 2b castle, 3b promo,  1b double, 1b ep
            int flags = 0;
            if (piece == 1)
            {
                //promo
                //default to queen
                if (to < 8 && 0 <= to)
                    flags |= 1 << 2;
                //doublemove
                else if ((48 <= from && from < 56) && (32 <= to && to < 40))
                    flags |= 1 << 1;
            }
            ulong p = (ulong)1 << to; //bit on the target square
            if ((p & Bmask) != 0) //capture, duh
            {
                flags |= 1 << 7;
                for (int i = 7; i < 12; ++i) //gotta scan through black bitboards and figure out the type of captured piece
                {
                    if ((p & BitBoards[i]) != 0)
                        piece |= i << 4; //sets the upper 4 bits to a type of captured piece
                }
            }


            uint res = (uint)((piece << 24) | (from << 16) | (to << 8) | (flags));
            //sw.Stop();
            //Console.WriteLine("{0} {1:X}",sw.Elapsed,res);

            return res;
        }

        static List<uint> MoveGeneration_White(ulong[] BBs)
        {
            Stopwatch sw = new Stopwatch();
            //sw.Start();

            List<uint> moves = new List<uint>();
            uint move;
            ulong amv;
            //searches through bitboards to find a suitable piece
            for (int i = 0; i < 64; ++i)
            {
                //a white piece is here
                if (Bit(Wmask, i))
                {
                    //what kind of piece is it
                    for (int j = 0; j < 6; ++j)
                    {
                        if (Bit(BBs[j], i))
                        //if (Bit(BitBoards[j], i))
                        {
                            //sw.Start();
                            amv = GenMoves(i, j, true, Wmask, Bmask, Block);
                            /*sw.Stop();
                            Console.WriteLine(sw.Elapsed);
                            sw.Reset();*/
                            //received a bitboard of possible squares to move on
                            ulong push = 1;
                            for (int k = 0; k < 64; ++k)
                            {

                                if ((amv & push) != 0) //this square can be moved on
                                {
                                    move = BuildMoveWhite(j, i, k);
                                    moves.Add(move);

                                    if ((move & (1 << 2)) != 0) //promotion - all promotion type pieces
                                    {
                                        //lowest bit signalizes promotion, following ones type: 00 queen, 01 rook, 10 bishop, 11 knight; in decimal, 1357 respectively
                                        move |= (1 << 3); //rook
                                        moves.Add(move);
                                        move ^= (3 << 3); //bishop
                                        moves.Add(move);
                                        move |= (1 << 3); //knight
                                        moves.Add(move);
                                    }
                                }

                                push <<= 1;
                            }
                        }
                    }
                }

            }

            //ep

            //sw.Start();
            int epsq = (Position >> 8) & 0b111111;
            if ((epsq != 0) && Bit(BitBoards[BB_p], epsq + 8))
            {
                int[] eps = EPGen(epsq, true);
                foreach (var e in eps)
                {//if the capture exists, the 7th bit must be set
                    if ((e & (1 << 7)) != 0)
                        moves.Add((uint)e);
                }
            }
            /*sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();*/

            //sw.Start();

            byte c = Castling_White(Position);

            //sets the 5th or 6th bit
            if ((c & 1) != 0)
            {
                move = 1 << 5;
                moves.Add(move);
            }
            if ((c & 2) != 0)
            {
                move = 1 << 6;
                moves.Add(move);
            }



            /*sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            sw.Reset();*/
            return moves;
        }


        static List<uint> CapsGeneration_White(ulong[] BBs)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<uint> moves = new List<uint>();
            uint move;
            ulong amv;
            //searches through bitboards to find a suitable piece
            for (int i = 0; i < 64; ++i)
            {
                if (Bit(Wmask, i))
                {
                    for (int j = 0; j < 6; ++j)
                    {
                        if (Bit(BBs[j], i))
                        {
                            amv = GenCaptures(i, j, true, Wmask, Bmask, Block);
                            ulong push = 1;
                            for (int k = 0; k < 64; ++k)
                            {

                                if ((amv & push) != 0) //this square can be moved on
                                {
                                    move = BuildMoveWhite(j, i, k);
                                    moves.Add(move);

                                    if ((move & (1 << 2)) != 0) //promotion - all promotion type pieces
                                    {
                                        //lowest bit signalizes promotion, following ones type: 00 queen, 01 rook, 10 bishop, 11 knight
                                        move |= (1 << 3); //rook
                                        moves.Add(move);
                                        move ^= (3 << 3); //bishop
                                        moves.Add(move);
                                        move |= (1 << 3); //knight
                                        moves.Add(move);
                                    }
                                }

                                push <<= 1;
                            }
                        }
                    }
                }

            }

            //ep
            int epsq = (Position >> 8) & 0b111111;
            if ((epsq != 0) && Bit(BitBoards[BB_p], epsq + 8))
            {
                int[] eps = EPGen(epsq, true);
                foreach (var e in eps)
                {//if the capture exists, the 7th bit must be set
                    if ((e & (1 << 7)) != 0)
                        moves.Add((uint)e);
                }
            }

            sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            sw.Reset();
            return moves;
        }


        static uint BuildMoveBlack(int piece, int from, int to)
        {
            //Warning: Does not consider capturing king
            //B1 = piece, B2 = From, B3 = To, B4 = Flags
            //Flags: 1b capture, 2b castle, 3b promo,  1b double, 1b ep
            int flags = 0;
            if (piece == 7)
            {
                //promo
                //default to queen
                if (56 <= to && to < 64)
                    flags |= 1 << 2;
                //double
                else if ((8 <= from && from < 16) && (24 <= to && to < 32))
                    flags |= 1 << 1;

            }
            ulong p = (ulong)1 << to;
            if ((p & Wmask) != 0) //capture, duh
            {
                flags |= 1 << 7;
                for (int i = 1; i < 6; ++i) //gotta scan through white bitboards and figure out the type of captured piece
                {
                    if ((p & BitBoards[i]) != 0)
                        piece |= i << 4; //sets the upper 4 bits to a type of captured piece
                }
            }

            uint res = (uint)((piece << 24) | (from << 16) | (to << 8) | (flags));
            return res;
        }




        static List<uint> MoveGeneration_Black(ulong[] BBs)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<uint> moves = new List<uint>();
            uint move;
            ulong amv;
            for (int i = 0; i < 64; ++i)
            {
                if (Bit(Bmask, i))
                {
                    for (int j = 6; j < 12; ++j)
                    {
                        if (Bit(BBs[j], i))
                        {
                            amv = GenMoves(i, j, false, Wmask, Bmask, Block);
                            ulong push = 1;
                            for (int k = 0; k < 64; ++k)
                            {

                                if ((amv & push) != 0) //this square can be moved on
                                {
                                    move = BuildMoveBlack(j, i, k);
                                    moves.Add(move);

                                    if ((move & (1 << 2)) != 0) //promotion - all promotion type pieces
                                    {
                                        //lowest bit signalizes promotion, following ones type: 00 queen, 01 rook, 10 bishop, 11 knight
                                        move |= (1 << 3); //rook
                                        moves.Add(move);
                                        move ^= (3 << 3); //bishop
                                        moves.Add(move);
                                        move |= (1 << 3); //knight
                                        moves.Add(move);
                                    }
                                }

                                push <<= 1;
                            }
                        }
                    }
                }

            }

            //ep
            int epsq = (Position >> 8) & 0b111111;
            if ((epsq != 0) && Bit(BitBoards[BB_P], epsq - 8))
            {
                int[] eps = EPGen(epsq, false);
                foreach (var e in eps)
                {//if the capture exists, the 7th bit must be set
                    if ((e & (1 << 7)) != 0)
                        moves.Add((uint)e);
                }
            }

            //castling

            byte c = Castling_Black(Position);
            if ((c & 1) != 0)
            {
                move = 1 << 5;
                moves.Add(move);
            }
            if ((c & 2) != 0)
            {
                move = 1 << 6;
                moves.Add(move);
            }


            sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            sw.Reset();
            return moves;
        }

        static List<uint> CapsGeneration_Black(ulong[] BBs)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<uint> moves = new List<uint>();
            uint move;
            ulong amv;
            for (int i = 0; i < 64; ++i)
            {
                if (Bit(Bmask, i))
                {
                    for (int j = 6; j < 12; ++j)
                    {
                        if (Bit(BBs[j], i))
                        {
                            amv = GenCaptures(i, j, false, Wmask, Bmask, Block);
                            ulong push = 1;
                            for (int k = 0; k < 64; ++k)
                            {

                                if ((amv & push) != 0) //this square can be moved on
                                {
                                    move = BuildMoveBlack(j, i, k);
                                    moves.Add(move);

                                    if ((move & (1 << 2)) != 0) //promotion - all promotion type pieces
                                    {
                                        //lowest bit signalizes promotion, following ones type: 00 queen, 01 rook, 10 bishop, 11 knight
                                        move |= (1 << 3); //rook
                                        moves.Add(move);
                                        move ^= (3 << 3); //bishop
                                        moves.Add(move);
                                        move |= (1 << 3); //knight
                                        moves.Add(move);
                                    }
                                }

                                push <<= 1;
                            }
                        }
                    }
                }

            }

            //ep
            int epsq = (Position >> 8) & 0b111111;
            if ((epsq != 0) && Bit(BitBoards[BB_P], epsq - 8))
            {
                int[] eps = EPGen(epsq, false);
                foreach (var e in eps)
                {//if the capture exists, the 7th bit must be set
                    if ((e & (1 << 7)) != 0)
                        moves.Add((uint)e);
                }
            }


            sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            sw.Reset();
            return moves;
        }
        public static readonly char[] pieces = new char[12] { 'K', 'P', 'N', 'B', 'R', 'Q', 'k', 'p', 'n', 'b', 'r', 'q' };

        public static bool Bit(ulong bitboard, int Pos)
        {
            ulong x = ((ulong)1 << Pos);
            ulong f = bitboard & x;
            return (f != 0);
        }
        static void Printout(ulong bitboard)
        {
            Console.WriteLine();
            for (int i = 0; i < 8; i++) //i is for rows, j is for columns
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write("+---");
                }
                Console.WriteLine("+");
                for (int j = 0; j < 8; j++)
                {
                    Console.Write("| ");
                    //if occupied, write something here... later
                    if (Bit(bitboard, 8 * i + j))
                        Console.Write("X");
                    else
                        Console.Write(" ");
                    Console.Write(" ");
                }
                Console.WriteLine("|");
            }
            for (int j = 0; j < 8; j++)
            {
                Console.Write("+---");
            }
            Console.WriteLine("+");
        }

        static void Printout(ulong[] bitboards)
        {
            Console.WriteLine();
            for (int i = 0; i < 8; i++) //i is for rows, j is for columns
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write("+---");
                }
                Console.WriteLine("+");
                for (int j = 0; j < 8; j++)
                {
                    Console.Write("| ");
                    char piece = ' ';
                    //if occupied, write something here...
                    for (int k = 0; k < bitboards.Length; k++)
                    {
                        if (Bit(bitboards[k], 8 * i + j))
                        {
                            piece = pieces[k];
                            break;
                        }
                    }
                    Console.Write(piece);
                    Console.Write(" ");
                }
                Console.WriteLine("|");
            }
            for (int j = 0; j < 8; j++)
            {
                Console.Write("+---");
            }
            Console.WriteLine("+");
        }
        /*static void CheckMove(int idx, ulong Wmask, ulong Bmask)
        {
            ulong Block = Wmask | Bmask;
            ulong movs;
            Printout(Block);

            Console.WriteLine("King:");
            movs = MovesKing(idx, Wmask, Bmask, Block, true);

            Printout(Block | ((ulong)(1) << idx));
            Printout(Block | movs);

            Console.WriteLine("Knight:");
            movs = MovesKnight(idx, Wmask, Bmask, Block, true);
            Printout(Block | ((ulong)(1) << idx));
            Printout(Block | movs);

            Console.WriteLine("Bishop:");
            movs = MovesBishop(idx, Wmask, Bmask, Block, true);
            Printout(Block | ((ulong)(1) << idx));
            Printout(Block | movs);

            Console.WriteLine("Rook:");
            movs = MovesRook(idx, Wmask, Bmask, Block, true);
            Printout(Block | ((ulong)(1) << idx));
            Printout(Block | movs);

            Console.WriteLine("Queen:");
            movs = MovesQueen(idx, Wmask, Bmask, Block, true);
            Printout(Block | ((ulong)(1) << idx));
            Printout(Block | movs);
        }*/
        public static ulong[] BitBoards;
        public ulong[] GetBitBoards()
        {
            return BitBoards;
        }

        //public static ulong[] BitBoards = new ulong[12];
        static ulong[] CreateBBoards(char[] board)
        { //creates the basic bitboards
            ulong[] Bitboards = new ulong[12];
            for (int i = 0; i < board.Length; ++i)
            {
                switch (board[i])
                {
                    case 'K':
                        Bitboards[0] |= ((ulong)(1) << i);
                        break;
                    case 'P':
                        Bitboards[1] |= ((ulong)(1) << i);
                        break;
                    case 'N':
                        Bitboards[2] |= ((ulong)(1) << i);
                        break;
                    case 'B':
                        Bitboards[3] |= ((ulong)(1) << i);
                        break;
                    case 'R':
                        Bitboards[4] |= ((ulong)(1) << i);
                        break;
                    case 'Q':
                        Bitboards[5] |= ((ulong)(1) << i);
                        break;

                    case 'k':
                        Bitboards[6] |= ((ulong)(1) << i);
                        break;
                    case 'p':
                        Bitboards[7] |= ((ulong)(1) << i);
                        break;
                    case 'n':
                        Bitboards[8] |= ((ulong)(1) << i);
                        break;
                    case 'b':
                        Bitboards[9] |= ((ulong)(1) << i);
                        break;
                    case 'r':
                        Bitboards[10] |= ((ulong)(1) << i);
                        break;
                    case 'q':
                        Bitboards[11] |= ((ulong)(1) << i);
                        break;
                }
            }
            return Bitboards;
        }
        static int[] CreatePieces(char[] board)
        {
            //reads the board and makes an array of all pieces; one piece = one integer
            int[] pieces = new int[64];
            byte currlength = 0;
            int piece; //LSB: color-type, square, value, value
            byte colortype;
            byte square;
            short value;
            for (byte i = 0; i < board.Length; ++i)
            {
                if (board[i] == '\0' || board[i] == '-') //empty sq
                    continue;
                colortype = Types[board[i]];
                value = (short)Values[colortype];
                square = (byte)Indices[i];
                piece = value << 16 | square << 8 | colortype;
                pieces[currlength] = piece;
                currlength++;
            }
            return pieces;
        }
        static readonly string[] Squares = new string[]
        {
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1"
        };
        static string DecodeMove(int move)
        {
            if (move == emptymove)
                return "*";
            if ((move & (3 << 5)) != 0) //castling
            {
                if ((move & (1 << 6)) != 0) //kside
                    return "0-0";
                else
                    return "0-0-0";
            }

            bool capture = ((move & (1 << 7)) != 0);
            int promotion = ((move >> 2) & 7); //3 last bits

            int to = (move >> 8) & ((1 << 8) - 1);
            int from = (move >> 16) & ((1 << 8) - 1);
            int piece = (move >> 24);
            piece &= ((1 << 4) - 1);
            string f = Squares[from];
            string t = Squares[to];
            string p = pieces[piece].ToString();
            string m = "";
            switch (promotion)
            {
                case 0:
                    break;
                case 0b001:
                    m = "=Q";
                    break;
                case 0b011:
                    m = "=R";
                    break;
                case 0b101:
                    m = "=B";
                    break;
                case 0b111:
                    m = "=N";
                    break;
            }
            char c = capture ? 'x' : '-';

            string res;
            res = p + f + c + t + m;
            return res;
        }
        static int NameInd(string name)
        {
            int col = name[0] - 'a';
            int row = 8 - (name[1] - 48);
            return 8 * row + col;
        }
        static string IndName(int index)
        {
            int col = index % 8;
            int row = index / 8;
            char co = (char)('a' + col);
            return co.ToString() + (8 - row).ToString();
        }

        static uint IncodeMove(string move, bool white) //very simple move naming, like e2e4 etc
        {
            uint mo = 0;
            if (move == "0-0")
            {
                return 0x40;
            }
            else if (move == "0-0")
            {
                return 0x20;
            }
            string f = move[0].ToString() + move[1].ToString();
            string t = move[2].ToString() + move[3].ToString();
            int from = NameInd(f);
            int to = NameInd(t);
            int piece = 13;
            int captur = 0;
            //find out name of a piece
            for (int i = 0; i <= 12; ++i)
            {
                if (Bit(BitBoards[i], from))
                {
                    piece = i;
                    break;
                }
            }
            if (white)
                return BuildMoveWhite(piece, from, to);
            else
                return BuildMoveBlack(piece, from, to);
        }
        //these ones change the zobrist hash based on the given position; 
        //at start, nullifies them, at the end, adjusts them accordingly
        static void FixPositionHash()
        {
            for (int i = 0; i < 5; ++i)
            {
                if (Bit(Position, i)) //if there is a bit, onify the given hash
                {
                    CurrentHash ^= HashSeed[772 - i];
                }
            }
            return;
        }
        static void XH(int where) //simplyfing the xor in hashing procedure
        {
            CurrentHash ^= HashSeed[where];
        }
        static uint[] MakeMove(uint move, bool white)
        {
            //B1 = piece, B2 = From, B3 = To, B4 = Flags
            //Flags: 1b capture, 2b castle, 3b promo,  1b double, 1b ep
            uint[] memo = new uint[2];
            memo[0] = move;
            memo[1] = (uint)Position;

            FixPositionHash();

            uint piece = (move >> 24) & ((1 << 4) - 1);
            int blankmoves = (Position >> 24) & (0b00_111111); //bits 2-7 (MSB) signalize moves which do not progress
            Position &= ~(0b00_111111 << 24); //nullifies the blank move count
            if (piece == 1 || piece == 7)
                blankmoves = 0;
            else
                blankmoves++; //if not nullified, the count increases by 1 (is nullified by pawn move or capture in general)

            if (move == emptymove) //secret code for empty move
            {
                Position ^= (1 << 4); //flip side, that's it

                FixPositionHash();
                return memo;
            }

            
            if (Bit(move, 5) || Bit(move, 6)) //castling
            {
                if (Bit(move, 5))//qside
                {
                    //moves rook together with the king
                    if (white)
                    {
                        BitBoards[BB_K] ^= ((ulong)1 << 60 | (ulong)1 << 58);
                        BitBoards[BB_R] ^= ((ulong)1 << 56 | (ulong)1 << 59);

                        CurrentHash ^= HashSeed[BB_K * 64 + 60];
                        CurrentHash ^= HashSeed[BB_K * 64 + 58];
                        CurrentHash ^= HashSeed[BB_R * 64 + 56];
                        CurrentHash ^= HashSeed[BB_R * 64 + 59];

                        Position &= ~(3 << 2);
                        Wking = 58;
                    }
                    else
                    {
                        BitBoards[BB_k] ^= ((ulong)1 << 4 | (ulong)1 << 2);
                        BitBoards[BB_r] ^= ((ulong)1 << 0 | (ulong)1 << 3);

                        CurrentHash ^= HashSeed[BB_k * 64 + 4];
                        CurrentHash ^= HashSeed[BB_k * 64 + 2];
                        CurrentHash ^= HashSeed[BB_r * 64 + 0];
                        CurrentHash ^= HashSeed[BB_r * 64 + 3];



                        Position &= ~3;
                        Bking = 2;
                    }

                }
                else if (Bit(move, 6)) //kside
                {
                    if (white)
                    {
                        BitBoards[BB_K] ^= ((ulong)1 << 60 | (ulong)1 << 62);
                        BitBoards[BB_R] ^= ((ulong)1 << 63 | (ulong)1 << 61);

                        CurrentHash ^= HashSeed[BB_K * 64 + 60];
                        CurrentHash ^= HashSeed[BB_K * 64 + 62];
                        CurrentHash ^= HashSeed[BB_R * 64 + 63];
                        CurrentHash ^= HashSeed[BB_R * 64 + 61];



                        Position &= ~(3 << 2);
                        Wking = 62;
                    }
                    else
                    {
                        BitBoards[BB_k] ^= ((ulong)1 << 4 | (ulong)1 << 6);
                        BitBoards[BB_r] ^= ((ulong)1 << 7 | (ulong)1 << 5);

                        CurrentHash ^= HashSeed[BB_K * 64 + 4];
                        CurrentHash ^= HashSeed[BB_K * 64 + 6];
                        CurrentHash ^= HashSeed[BB_R * 64 + 7];
                        CurrentHash ^= HashSeed[BB_R * 64 + 5];


                        Position &= ~3;
                        Bking = 6;
                    }
                }
                Remask();
                Position ^= (1 << 4); //flip side
                Position |= (blankmoves << 24);

                FixPositionHash();
                return memo;
            }

            int from = ((int)move >> 16) & ((1 << 8) - 1);
            int to = ((int)move >> 8) & ((1 << 8) - 1);


            if (Bit(move, 0)) //EP
            {
                if (white)
                {
                    BitBoards[BB_P] ^= (((ulong)1 << from) | ((ulong)1 << to)); //pushes pawn on the new position
                    XH(BB_P * 64 + from);
                    XH(BB_P * 64 + to);



                    BitBoards[BB_p] &= ~((ulong)1 << (to + 8)); //deletes black pawn in front of the "to" square of white pawn
                    XH(BB_p * 64 + to + 8);
                }
                else
                {
                    BitBoards[BB_p] ^= (((ulong)1 << from) | ((ulong)1 << to)); //pushes pawn on the new position
                    XH(BB_P * 64 + from);
                    XH(BB_P * 64 + to);

                    BitBoards[BB_P] &= ~((ulong)1 << (to - 8)); //deletes white pawn behind the "to" square of black pawn
                    XH(BB_p * 64 + to - 8);
                }

                //terminate, since there is nothing else special on EP

                Remask();
                //detrigger en-passant
                Position &= ~(0b111111 << 8);
                Position ^= (1 << 4); //flips side
                Position |= (blankmoves << 24);

                FixPositionHash();
                return memo;
            }

            //normal move
            //nullifies bit on the bitboard of the corresponding piece

            BitBoards[piece] &= ~((ulong)1 << from); //nullify from

            XH((int)(piece * 64) + from);

            BitBoards[piece] |= ((ulong)1 << to); //onify to
            XH((int)(piece * 64) + to);

            //detrigger en-passant
            Position &= ~(0b111111 << 8);
            //doublepush triggers en-passant 
            if ((move & 2) != 0)
            {
                Position |= ((to + (white ? 8 : -8)) << 8); //marks the square behind pushed pawn
            }


            //promotion: formula for the id of new piece is "5 - value (+6 if black)"
            if ((move & 4) != 0)
            {
                int prompiece = (white ? 5 : 11) - (((int)move & 24) >> 3);
                BitBoards[prompiece] |= ((ulong)1 << to);
                XH((int)(prompiece * 64) + to);
                BitBoards[piece] &= ~((ulong)1 << to);
                XH((int)(piece * 64) + to);
            }

            //capture - nullify the bits on opponent's BBoard

            if ((move & 1 << 7) != 0)
            {
                blankmoves = 0;
                uint captur = (move >> 28);
                BitBoards[captur] &= ~((ulong)1 << to);
                XH((int)(captur * 64) + to);
            }

            //transport king

            if (piece == 0)
                Wking = to;
            else if (piece == 6)
                Bking = to;

            //fix castling rights

            switch (from)
            {
                case 0:
                    Position &= ~1;
                    break;
                case 7:
                    Position &= ~2;
                    break;
                case 56:
                    Position &= ~4;
                    break;
                case 63:
                    Position &= ~8;
                    break;
                case 4:
                    Position &= ~3;
                    break;
                case 60:
                    Position &= ~12;
                    break;
                default:
                    break;
            }
            switch (to)
            {
                case 0:
                    Position &= ~1;
                    break;
                case 7:
                    Position &= ~2;
                    break;
                case 56:
                    Position &= ~4;
                    break;
                case 63:
                    Position &= ~8;
                    break;
                case 4:
                    Position &= ~3;
                    break;
                case 60:
                    Position &= ~12;
                    break;
                default:
                    break;
            }

            Remask();
            Position ^= (1 << 4); //flips side
            Position |= (blankmoves << 24);

            FixPositionHash();
            return memo;
        }

        static uint UndoMove(uint[] memo, bool white)
        {
            //in this case, the "position" is not modified in any way - simply reloads the state from the return of MakeMove.
            uint move = memo[0];
            uint piece = (move >> 24) & ((1 << 4) - 1);
            FixPositionHash();

            if (move == emptymove) //secret code for empty move
            {
                Position ^= (1 << 4); //flip side, that's it

                FixPositionHash();
                return move;
            }
            if (Bit(move, 5) || Bit(move, 6)) //castling
            {
                if (Bit(move, 5))//qside
                {
                    //moves rook together with the king, retracts castling rights
                    if (white)
                    {
                        BitBoards[BB_K] ^= ((ulong)1 << 60 | (ulong)1 << 58);

                        XH(BB_K * 64 + 60);
                        XH(BB_K * 64 + 60);

                        BitBoards[BB_R] ^= ((ulong)1 << 56 | (ulong)1 << 59);

                        XH(BB_R * 64 + 56);
                        XH(BB_R * 64 + 59);
                        //Position |= (3 << 2);
                        Wking = 60;
                    }
                    else
                    {
                        BitBoards[BB_k] ^= ((ulong)1 << 4 | (ulong)1 << 2);

                        XH(BB_k * 64 + 4);
                        XH(BB_k * 64 + 2);

                        BitBoards[BB_r] ^= ((ulong)1 << 0 | (ulong)1 << 3);

                        XH(BB_r * 64 + 0);
                        XH(BB_r * 64 + 3);
                        //Position |= 3;
                        Bking = 4;
                    }

                }
                else if (Bit(move, 6)) //kside
                {
                    if (white)
                    {
                        BitBoards[BB_K] ^= ((ulong)1 << 60 | (ulong)1 << 62);

                        XH(BB_K * 64 + 60);
                        XH(BB_K * 64 + 62);

                        BitBoards[BB_R] ^= ((ulong)1 << 63 | (ulong)1 << 61);

                        XH(BB_R * 64 + 63);
                        XH(BB_R * 64 + 61);

                        //Position &= ~(3 << 2);
                        Wking = 60;
                    }
                    else
                    {
                        BitBoards[BB_k] ^= ((ulong)1 << 4 | (ulong)1 << 6);

                        XH(BB_k * 64 + 4);
                        XH(BB_k * 64 + 6);

                        BitBoards[BB_r] ^= ((ulong)1 << 7 | (ulong)1 << 5);

                        XH(BB_r * 64 + 7);
                        XH(BB_r * 64 + 5);

                        //Position &= ~3;
                        Bking = 4;
                    }
                }
                Remask();
                Position = (int)memo[1]; //refreshes the previous position flags

                FixPositionHash();
                return move;
            }

            int from = ((int)move >> 16) & ((1 << 8) - 1);
            int to = ((int)move >> 8) & ((1 << 8) - 1);

            if (Bit(move, 0)) //EP
            {
                if (white)
                {
                    BitBoards[BB_P] ^= (((ulong)1 << from) | ((ulong)1 << to)); //returns pawn on the original position
                    XH((int)(BB_P * 64) + from);
                    XH((int)(BB_P * 64) + to);
                    BitBoards[BB_p] |= ((ulong)1 << (to + 8)); //returns black pawn in front of the "from" square of white pawn
                    XH((int)(BB_p * 64) + to + 8);
                }
                else
                {
                    BitBoards[BB_p] ^= (((ulong)1 << from) | ((ulong)1 << to)); //returns pawn on the original position
                    XH((int)(BB_p * 64) + from);
                    XH((int)(BB_p * 64) + to);
                    BitBoards[BB_P] |= ((ulong)1 << (to - 8)); //returns white pawn behind the "from" square of black pawn
                    XH((int)(BB_P * 64) + to - 8);
                }

                //terminate, since there is nothing else special on EP

                Remask();
                Position = (int)memo[1]; //refreshes the previous position flags

                FixPositionHash();
                return move;
            }


            //normal move without capture

            //onifies bit on the bitboard of the corresponding piece
            BitBoards[piece] |= ((ulong)1 << from); //onify from
            XH((int)(piece * 64) + from);

            //changes the bit on the resulting square to 0
            BitBoards[piece] &= ~((ulong)1 << to); //nullify to
            XH((int)(piece * 64) + to);

            if ((move & 1 << 7) != 0) //capture - onify the bits on opponent's BBoard
            {
                uint captur = (move >> 28);
                BitBoards[captur] |= ((ulong)1 << to);
                XH((int)(captur * 64) + to);
            }

            //transport king

            if (piece == 0)
                Wking = from;
            else if (piece == 6)
                Bking = from;

            //promotion
            if ((move & 4) != 0)
            {
                //pawn has returned to the original place, nothing has been done with the promoted piece
                int prompiece = (white ? 5 : 11) - (((int)move & 24) >> 3);
                BitBoards[prompiece] &= ~((ulong)1 << to);
                XH((int)(prompiece * 64) + to);
            }

            //fix castling rights

            Remask();
            Position = (int)memo[1]; //refreshes the previous position flags

            FixPositionHash();
            return move;
        }

        static ulong[] GenRandomNumbers(int howmany, int seed)
        {
            var randoms = new ulong[howmany];
            var rng = new ULongRandom(seed);
            for (int i = 0; i < howmany; ++i)
            {
                randoms[i] = rng.Next();
                if (randoms[i] == 0)
                    randoms[i] += 1;
            }
            return randoms;
        }
        const int randomSeed = 23;
        static readonly ulong[] HashSeed = GenRandomNumbers(784, randomSeed);
        static ulong HashPosition(ulong[] rgn) //Zobrist hashing
        {
            ulong hash = 0;
            int i = 0;
            for (int j = 0; j < 12; ++j)
            { //pieces
                for (int k = 0; k < 64; ++k)
                {
                    if (Bit(BitBoards[j], k))
                    {
                        hash ^= rgn[i];
                    }
                    ++i;
                }
            }
            if (Bit(Position, 4))
            { //beginning player
                hash ^= rgn[i];
            }
            ++i;
            for (int j = 3; j >= 0; --j)
            { //castling
                if (Bit(Position, j))
                {
                    hash ^= rgn[i];
                }
                ++i;
            }


            if (Bit(Position, 12)) //ep is at rank 6 or 3
            {
                int epfile = (Position >> 8) & 0b000111; //bottom 3 bits of ep
                for (int j = 0; j < 8; ++j)
                {
                    if (j == epfile)
                    {
                        hash ^= rgn[i];
                    }
                    ++i;
                }
            }


            return hash;
        }
        public class Hashentry
        {
            public ulong zobrist;
            public int depth;
            public int flag;
            public int eval;
            public int ancient;
            public uint move;
            public uint[] pv;
            public ulong WM;
            public ulong BM;

            public Hashentry(ulong zobrist, int depth, int flag,
                             int eval, int ancient, uint move)
            {
                this.zobrist = zobrist;
                this.depth = depth;
                this.flag = flag;
                this.eval = eval;
                this.ancient = ancient;
                this.move = move;
            }
            public Hashentry()
            {

            }
            public Hashentry(int depth)
            {
                this.pv = new uint[depth];
            }
        }

        public static int AlphaBeta_TT(int currdepth, int maxdepth, int alpha, int beta, bool white, LINE pline, bool evaluation_system)
        {

            //var ss = new Stopwatch();
            //ss.Start();

            int curreval;
            LINE line = new LINE(maxdepth);
            bool end = true;
            if (currdepth >= maxdepth)
            {
                NodesSearched += 1;
                pline.totalmoves = 0;
                int ira = Quisce(int.MinValue + 10, int.MaxValue - 10, white, evaluation_system);
                return ira;
            }
            uint[] mvs = white ? MoveGeneration_White(BitBoards).ToArray() : MoveGeneration_Black(BitBoards).ToArray();
            SortMoves(mvs);

            NodesSearched += 1;
            for (int i = 0; i < mvs.Length; ++i)
            {
                //int checkito = Evaluation();
                uint donemove = mvs[i];
                uint[] mvm = MakeMove(donemove, white);

                //what if move is illegal
                if (Attacked(white ? Wking : Bking, Wmask, Bmask, Block, !white, BitBoards))
                {
                    UndoMove(mvm, white);
                    continue;
                }
                end = false;

                curreval = -AlphaBeta_TT(currdepth + 1, maxdepth, -beta, -alpha, !white, line, evaluation_system);

                UndoMove(mvm, white);

                if (curreval >= beta)
                {
                    return beta;
                }
                if (curreval > alpha)
                {

                    alpha = curreval;
                    //change principal variation

                    pline.argmove[0] = donemove;
                    //copy this:
                    /*
                        line.argmove, //which move we start
                        pline.argmove + 1, //max moves from current line, increased length
                        line.totalmoves * sizeof(uint) //how many of the integers 
                     */
                    //first element of pline stays, and the whole "line" gets added
                    int s = 0;
                    for (int l = 0; line.argmove[l] != 0; ++l)
                    {
                        s++;
                    }
                    Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                    pline.totalmoves = line.totalmoves + 1;
                }
            }
            if (end)
            { //if player has no legal moves, it's mate or stalemate
                return -EndMateEval(white, 1 + currdepth);
            }
            if (InsufMaterial()||NoProgress())
            {
                return 0;
            }
            return alpha;
        }
        public static int AlphaBeta_Rewritten(int currdepth, int maxdepth, int alpha, int beta, bool white, LINE pline, bool evaluation_system)
        {
            NodesSearched += 1;
            int AlphaOrig = alpha;
            LINE line = new LINE(maxdepth);
            Hashentry entry;
            bool end = true;


            if (currdepth >= maxdepth) //or end...
            {
                return Quisce(alpha, beta, white, evaluation_system);
                //return Evaluation();
            }

            bool Intable = (TranspoTable[currdepth].TryGetValue((ushort)(CurrentHash), out entry)); //checks the transpo table
            if (Intable && /*entry.depth == currdepth &&*/ entry.zobrist == CurrentHash && entry.WM == Wmask && entry.BM == Bmask) //check the whole hash, in case of collishns
            {
                if (entry.flag == 1) //exact 
                {
                    return entry.eval;
                }
                else if (entry.flag == 2) //upper
                {
                    beta = Math.Min(beta, entry.eval);
                }
                else if (entry.flag == 0) //lower
                {
                    if (entry.eval > alpha)
                    {
                        alpha = entry.eval;
                        if (alpha <= beta)
                        {
                            //change principal variation
                            int borderindex = maxdepth - currdepth;
                            int t = 0;
                            for (int l = 0; entry.pv[l] != 0; ++l)
                            {
                                t++;
                            }
                            //pline.argmove[0] = entry.move;

                            //copy this:
                            /*
                                line.argmove, //which move we start
                                pline.argmove + 1, //max moves from current line, increased length
                                line.totalmoves * sizeof(uint) //how many of the integers 
                             */
                            //first element of pline stays, and the whole "line" gets added
                            int s = 0;
                            for (int l = 0; line.argmove[l] != 0; ++l)
                            {
                                s++;
                            }
                            pline.argmove = entry.pv;
                            Array.Copy(line.argmove, 0, pline.argmove, t, s);
                            //Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                            pline.totalmoves = line.totalmoves + 1;
                        }
                    }
                }
                if (alpha >= beta)
                {
                    return entry.eval;
                }
            }
            if (currdepth >= maxdepth) //or end...
            {
                return Quisce(alpha, beta, white, evaluation_system);
                //return Evaluation();
            }
            uint[] mvs = white ? MoveGeneration_White(BitBoards).ToArray() : MoveGeneration_Black(BitBoards).ToArray();
            Shuffle(mvs);
            SortMoves(mvs);
            uint currentBest = 0;
            for (int i = 0; i < mvs.Length; ++i)
            {
                if (entry != null && entry.depth > currdepth && (entry.flag == 2 || entry.flag == 1) && mvs[i] == entry.move)
                {
                    uint temp = mvs[i];
                    mvs[i] = mvs[0];
                    mvs[0] = temp;
                    //replaces with best move
                    break;
                }
            }
            uint donemove;
            int BestValue = int.MinValue + 10;
            foreach (uint move in mvs)
            {
                uint[] mvm = MakeMove(move, white);

                if (move == 0 || Attacked(white ? Wking : Bking, Wmask, Bmask, Block, !white, BitBoards))
                {
                    UndoMove(mvm, white);
                    continue;
                }
                //following only when move is legal
                end = false;

                donemove = move;
                int curreval;

                /*if (InsufMaterial()||NoProgress())
                {
                    staticEval = 0;
                }*/
                if (InsufMaterial())
                {
                    curreval = 0;
                }
                else if (NoProgress())
                {
                    curreval = 0;
                }
                else
                {
                    curreval = -AlphaBeta_Rewritten(currdepth + 1, maxdepth, -beta, -alpha, !white, line, evaluation_system);
                }

                UndoMove(mvm, white);

                if (curreval > BestValue)
                {
                    BestValue = curreval;
                    currentBest = move;
                }
                if (curreval >= beta)
                {
                    return beta;
                }
                if (curreval > alpha)
                {

                    alpha = curreval;
                    //change principal variation

                    pline.argmove[0] = currentBest;
                    //copy this:
                    /*
                        line.argmove, //which move we start
                        pline.argmove + 1, //max moves from current line, increased length
                        line.totalmoves * sizeof(uint) //how many of the integers 
                     */
                    //first element of pline stays, and the whole "line" gets added
                    int s = 0;
                    for (int l = 0; line.argmove[l] != 0; ++l)
                    {
                        s++;
                    }
                    Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                    pline.totalmoves = line.totalmoves + 1;
                }
            }
            if (end)
            { //if player has no legal moves, it's mate or stalemate
                return -EndMateEval(white, 1 + currdepth);
            }


            Hashentry next = new Hashentry(maxdepth);
            next.depth = currdepth;
            next.eval = BestValue;
            next.zobrist = CurrentHash;
            next.move = currentBest;
            next.WM = Wmask;
            next.BM = Bmask;
            if (BestValue <= AlphaOrig)
            {
                next.flag = 2;
            }
            else if (BestValue >= beta)
            {
                next.flag = 0;
            }
            else
            {
                next.flag = 1;
            }
            if (!TranspoTable[currdepth].ContainsKey((ushort)(next.zobrist)))
            {
                TranspoTable[currdepth].Add((ushort)next.zobrist, next);
            }


            return alpha;
        }

        public static bool Bit(int position, int j)
        {
            return (((position) & (1 << j)) != 0);
        }

        public static bool SqColor(int index) //true light false dark
        {
            ulong dark = (0xAA55AA55AA55AA55);
            return (Bit(dark, index));
        }


        public static string FenToStr(string fen)
        {
            string rew = "";
            string brd = "";
            int c = 0;
            foreach (char j in fen)
            {
                if (c == 64)
                    break;
                if (j == '/' || j == ' ')
                    continue;
                else if (48 <= j && j <= 58)
                {//if a number, write that many blanks
                    c += j - 48;
                    for (byte i = 0; i < j - 48; ++i)
                    {
                        brd += "-";
                    }
                }
                else
                {
                    c++;
                    brd += j.ToString(); //normal piece char
                }
            }

            for (int i = fen.Length - 1; fen[i] != '/'; --i)
            {
                if (fen[i] == ' ')
                    continue;
                else
                    rew = fen[i] + rew;
            }

            return brd + rew;
        }

        public static char[] TightFenToChar(string fen)
        {
            char[] cc = new char[64];
            int i;
            for (i = 0; i < 64; ++i)
            {
                cc[i] = fen[i];
                if (fen[i] == 'k')
                    Bking = i;
                else if (fen[i] == 'K')
                    Wking = i;
            }

            for (i = 64; i <= 67; ++i) //castling...
            {
                if (fen[i] == '-')
                    continue;
                else
                    Position |= (1 << (67 - i)); //sets the according bit
            }
            if (fen[69] != '-')
            {
                int ep = (fen[69] - 'a') + (8 - (fen[70] - 48)) * 8;
                //ep starts at bit 8
                Position |= (ep << 8);
            }

            bool zacina = fen[68] == 'w';
            if (zacina)
                Position |= 16;
            return cc;
        }


        public static void Shuffle<T>(T[] items)
        {
            Random rand = new Random();

            // For each spot in the array, pick
            // a random item to swap into that spot.
            for (int i = 0; i < items.Length - 1; i++)
            {
                int j = rand.Next(i, items.Length);
                T temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
        public static bool NoProgress()
        {
            //no progress: at least than 50 blank moves
            return (((Position >> 24) & (0b00111111)) >= 50);
        }
        public static bool InsufMaterial() // checks for draw by lack of material
        {
            //any pawns, rooks or queens? False
            if ((BitBoards[1] | BitBoards[4] | BitBoards[5] | BitBoards[7] | BitBoards[10] | BitBoards[11]) != 0) return false;
            bool material = false;
            //plus same-colored bishops or nothing?
            ulong bish = BitBoards[3] | BitBoards[9];
            if (bish == 0) //no bishops
            {
                ulong knig = BitBoards[2] | BitBoards[8];
                //plus only one knight? True
                for (int i = 0; i < 64; ++i)
                {
                    if (Bit(knig, i)) //there is a knight
                    {
                        //permits one knight, if there is another one found, position is not dead
                        if (material)
                            return false;
                        else
                            material = true;
                    }
                }
                return true;
            }
            if (((bish & 0xAA55AA55AA55AA55) == bish) || ((bish & 0xAA55AA55AA55AA55) == 0)) //all are either on white or black
            {
                return true;
            }
            return false;
        }


        public static int EndMate(bool white)
        {
            var swr = new StreamWriter("partie.txt", true);
            int result = 0;

            if (white && (Attacked(Wking, Wmask, Bmask, Block, false, BitBoards)))
            {
                Console.WriteLine("Mate for White, Black wins");
                swr.Write("# 0-1");
                result = 2;
            }
            else if ((!white) && (Attacked(Bking, Wmask, Bmask, Block, true, BitBoards)))
            {
                Console.WriteLine("Mate for Black, White wins");
                swr.Write("# 1-0");
                result = 3;
            }
            else
            {
                Console.WriteLine("Stalemate");
                swr.Write(" 1/2");
                result = 1;
            }
            swr.Close();
            RewritePartia("partie.txt");
            return result;
        }

        public static int EndMateEval(bool white, int dep)
        {
            if (white && (Attacked(Wking, Wmask, Bmask, Block, false, BitBoards)))
            {
                return int.MaxValue / dep; //dont ask why
            }
            else if ((!white) && (Attacked(Bking, Wmask, Bmask, Block, true, BitBoards)))
            {
                return int.MaxValue / dep;
            }
            else
            {
                return 0;
            }
        }
        public static void EndDraw()
        {
            var swr = new StreamWriter("partie.txt", true);
            Console.WriteLine("Draw due to insuf. material");
            swr.Write(" 1/2");
            swr.Close();

            RewritePartia("partie.txt");
        }

        public static string Rewrite(string notation)
        { //insert the "default" notation and transform it into pgn
            string newna = "";
            {
                for (int i = 0; i < notation.Length; ++i)
                {
                    if (notation[i] == '0' && (notation[i + 1] == '-' || notation[i - 1] == '-'))
                    {
                        newna += 'O';
                        continue;
                    }
                    if (notation[i] == 'P' || notation[i] == 'p')
                        continue; //not writing pawns
                    if (97 <= notation[i] && notation[i] <= 122 && notation[i] != 'x') //is lowercase
                    {
                        if (notation[i + 1] >= 57 || notation[i + 1] <= 48) //next one is not number (rank)
                        {
                            newna += ((char)(notation[i] - 32)).ToString();  //makes it an uppercase
                            continue;
                        }

                    }
                    newna += notation[i].ToString();
                }
            }
            return newna;

        }

        public static void RewritePartia(string paf)
        {
            //opens the "partie" and rewrites it

            var sr = new StreamReader(paf);
            string par = sr.ReadToEnd() + " "; //one space at the end to avoid overflows
            sr.Close();

            string pre = Rewrite(par);

            var sw = new StreamWriter("prepsana.txt", false);
            sw.Write(pre);
            sw.Close();
            return;
        }

        public static int LazyEvaluation(bool white) //evaluates position by simple factors
        {
            int eva = 0;
            int i;
            if (white)
            {
                for (i = 1; i < 6; ++i)
                {
                    int popcount = HammingWeight(BitBoards[i]);
                    eva += Values[i] * popcount;
                }

            }
            else
            {
                for (i = 7; i < 12; ++i)
                {
                    int popcount = HammingWeight(BitBoards[i]);
                    eva += Values[i] * popcount;
                }
            }


            return eva;

            //random evaluation favors more active positions
            //return evwhite - evblack + los.Next(-80, +80);

        }

        public static int Evaluation() //evaluates position based on many factors - lets start with material
        {
            int evwhite = 0;
            int evblack = 0;
            int i;
            ulong bita;


            /*for (i = 1; i < 6; ++i)
            {
                bita = 1;
                for (int j = 0; j < 64; ++j)
                {
                    if ((BitBoards[i] & bita) != 0) //for every single piece...
                    {
                        evwhite += Values[i]+Posit_Values[i][j];
                        //evwhite += Values[i]<<7;
                    }
                    bita <<= 1;
                }

            }*/

            /*for (i = 1; i < 6; ++i)
            {
                bita = 1;
                for (int j = 0; j < 64; ++j)
                {
                    if ((BitBoards[i+6] & bita) != 0) //for every single piece...
                    {
                        evblack += Values[i]+Posit_Values[i][j^0b111000]; //flips vertical coordinate
                        //evblack += Values[i]<<7;
                    }
                    bita <<= 1;
                }
            }*/


            bita = 1;
            for (int j = 0; j < 64; ++j)
            {
                if ((Wmask & bita) != 0)
                {
                    for (i = 1; i < 6; ++i)
                    {
                        if ((BitBoards[i] & bita) != 0) //for the corresponding piece...
                        {
                            evwhite += Values[i] + Posit_Values[i][j];
                            break;
                        }
                    }
                }
                bita <<= 1;
            }




            if (((BitBoards[3] & blacksquares) != BitBoards[3]) && (((BitBoards[3] & whitesquares) != BitBoards[3])))
            {
                evwhite += 40; //half a pawn for bishops on different colors
            }
            if (((BitBoards[9] & blacksquares) != BitBoards[9]) && (((BitBoards[9] & whitesquares) != BitBoards[9])))
            {
                evblack += 40;
            }


            bita = 1;
            for (int j = 0; j < 64; ++j)
            {
                if ((Bmask & bita) != 0)
                {
                    for (i = 1; i < 6; ++i)
                    {
                        if ((BitBoards[i + 6] & bita) != 0) //for the corresponding piece...
                        {
                            evblack += Values[i] + Posit_Values[i][j ^ 0b111000];
                            break;
                        }
                    }
                }
                bita <<= 1;
            }


            if (evwhite + evblack > 2000) //early or mid game
            {
                evwhite += Posit_Values[0][Wking];
                evblack += Posit_Values[0][Bking ^ 0b111000];
            }
            else
            {
                evwhite += Posit_Values[6][Wking];
                evblack += Posit_Values[6][Bking ^ 0b111000];
            }
            //return evwhite - evblack; 

            //random evaluation favors more active positions
            return evwhite - evblack;

        }



        public static void SortMoves(uint[] moves)
        { //sorts moves by captured and capturing strength
            var prios = new uint[moves.Length];
            uint attacker;
            uint captured;
            uint atstr;
            uint mov;
            uint prom;
            for (int i = 0; i < moves.Length; ++i)
            {
                mov = moves[i];
                atstr = 0;
                prom = ((mov >> 2) & (0b111));
                captured = (mov >> 28);
                if (captured != 0)
                {
                    attacker = (mov >> 24) & (0b1111);
                    atstr = Priorities[attacker];
                }
                if (prom != 0)
                {
                    prom ^= 0b110;
                }


                prios[i] = (prom << 8) | (captured << 4) | atstr;
            }

            Array.Sort(prios, moves);
            Array.Reverse(moves);
        }

        static long NodesSearched;
        static long TimeSpent;

        public static void Play(bool white)
        {

            uint mov = 0;
            int totmoves = 0;
            CurrentHash = HashPosition(HashSeed);
            //const int deph = 6;
            Console.WriteLine("Hloubka? ");
            int depth_base = int.Parse(Console.ReadLine());
            int deph = depth_base;
            TranspoTable = new Dictionary<uint, Hashentry>[depth_base + 4];
            for (int k = 0; k < TranspoTable.Length; ++k)
            {
                TranspoTable[k] = new Dictionary<uint, Hashentry>();
            }
            string nota = "";

            uint[] mvs;
            uint[] mvm;
            bool end;
            bool player = white;

            /*for(int i = 3; i <= 8; ++i)
            {
                LINE principalVariation = new LINE(i);
                int ab = AlphaBeta(0, i, int.MinValue+10, int.MaxValue-10, player, principalVariation);
                Console.WriteLine("Depth:{0} Eval: {1}", i, ab);
                //AllMovesEval(player, i);
            }*/

            var swplay = new Stopwatch();
            while (true)
            {

                NodesSearched = 0;
                TimeSpent = 0;

                //uint mov = 0; 
                /*if (player)
                {
                    mvs = MoveGeneration_White(BitBoards).ToArray();
                    mvs = CapsGeneration_White(BitBoards).ToArray();
                    for (int b = 0; b < mvs.Length; ++b)
                    {
                        Console.WriteLine(DecodeMove((int)(mvs[b])));
                    }
                }
                else
                {
                    mvs = MoveGeneration_Black(BitBoards).ToArray();
                    /*mvs = CapsGeneration_Black(BitBoards).ToArray();
                    for(int b = 0; b < mvs.Length; ++b)
                    {
                        Console.WriteLine(DecodeMove((int)(mvs[b])));
                    }
                }*/
                int ab = 0;
                end = true;
                LINE principalVariation = new LINE(deph + (player ? 0 : -1));
                for (int k = 0; k < TranspoTable.Length; ++k)
                {
                    TranspoTable[k] = new Dictionary<uint, Hashentry>();
                }
                if (player || !player)
                {


                    NodesSearched = 0;
                    swplay.Start();

                    // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, false); //no random
                    ab = AlphaBeta_Rewritten(0, deph + (player ? 0 : -1), int.MinValue + 10, int.MaxValue - 10, player, principalVariation, true); //both random
                                                                                                                                                   // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, player ? true : false); //white random
                                                                                                                                                   // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, player ? false : true); //black random

                    //ab = AlphaBeta_Rewritten(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, false);

                    swplay.Stop();
                    TimeSpent = swplay.ElapsedMilliseconds;
                    swplay.Reset();
                    //Shuffle(mvs);
                    //if (!player)
                    // Array.Reverse(mvs);

                    Console.WriteLine(ab);
                    end = true;
                    for (int i = 0; i < 1; ++i)
                    {

                        //mov = mvs[i];

                        //next part if ur a single player

                        //Console.WriteLine(DecodeMove((int)mov));
                        /*foreach (var mv in mvs)
                        {
                            Console.WriteLine(DecodeMove((int)mv));
                        }
                        Console.WriteLine("Zahraj");
                        string mo = Console.ReadLine();
                        //mov = mvs[3];
                        //mvm = MakeMove(mov, false);
                        uint mc = IncodeMove(mo, false);
                        mvm = MakeMove(mc, false);*/
                        mov = principalVariation.argmove[0];
                    }
                    if (mov == 0)
                    {
                        end = true;
                        EndMate(player);
                        return;
                    }

                    /*Console.WriteLine("{0:X}", CurrentHash);
                    mvm = MakeMove(mov, player);
                    Console.WriteLine("{0:X}", CurrentHash);
                    UndoMove(mvm, player);
                    Console.WriteLine("{0:X}", CurrentHash);
                    mvm = MakeMove(mov, player);
                    Console.WriteLine("{0:X}", CurrentHash);*/
                    mvm = MakeMove(mov, player);
                    if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                    {
                        UndoMove(mvm, player);
                        Console.WriteLine("Error: Illegal Move");
                        Console.WriteLine(DecodeMove((int)mov));
                        end = true;
                    }
                    else
                    {
                        end = false;
                    }
                }
                else
                {
                    Console.WriteLine("Tah");
                    string am = Console.ReadLine();
                    int from = am[0] - 'a' + 8 * (8 - (am[1] - 48));
                    int to = am[2] - 'a' + 8 * (8 - (am[3] - 48));
                    int piece = 0;
                    for (int n = 0; n < 12; ++n)
                    {
                        if (Bit(BitBoards[n], from))
                        {
                            piece = n;
                            break;
                        }


                    }

                    if (player)
                    {
                        mov = BuildMoveWhite(piece, from, to);
                    }
                    else
                    {
                        mov = BuildMoveBlack(piece, from, to);
                    }
                    mvm = MakeMove(mov, player);
                    if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                    {
                        UndoMove(mvm, player);
                        Console.WriteLine("Error: Illegal Move");
                        Console.WriteLine(DecodeMove((int)mov));
                        end = true;

                    }
                    else
                    {
                        end = false;
                    }
                }


                if (end)
                {
                    EndMate(player);
                    return;
                }



                Printout(BitBoards);
                Console.WriteLine("{0} {1}", IndName(Wking), IndName(Bking));
                //var sws = new Stopwatch();
                //sws.Start();

                int evwhite = LazyEvaluation(true);
                int evblack = LazyEvaluation(false);
                //Console.WriteLine(sws.Elapsed);
                Console.WriteLine(Evaluation());

                //sws.Reset();

                Console.WriteLine("Depth:{0} Eval: {1}", deph, ab);
                if (player || !player)
                {
                    for (int d = 0; d < principalVariation.argmove.Length; ++d)
                    {
                        Console.Write(DecodeMove((int)principalVariation.argmove[d]));
                        Console.Write(" ");
                    }
                }

                Console.WriteLine("Searched {0} nodes", NodesSearched);
                Console.WriteLine("Took {0} ms", TimeSpent);
                Console.WriteLine("{0} per second", NodesSearched * 1000 / (TimeSpent + 1));
                //Console.WriteLine(DecodeMove((int)mov));
                Console.WriteLine("");

                nota += DecodeMove((int)mov) + " ";
                nota += "{" + Evaluation().ToString() + "} ";

                var swr = new StreamWriter("partie.txt", false);
                swr.Write(nota);
                swr.Close();

                if (InsufMaterial())
                {
                    EndDraw();
                    return;
                }
                if (!player)
                    totmoves += 1;
                if (totmoves >= 300)
                {
                    EndDraw();
                    return;
                }

                ulong f = 0;
                for (int m = 2; m < 12; ++m)
                {
                    if (m == 6 || m == 7)
                        continue;
                    f |= BitBoards[m];
                }
                if (f == 0)
                {
                    deph = depth_base + 4;
                }
                else if (evwhite + evblack < 1200)
                {
                    deph = depth_base + 2;
                }
                else if (evwhite + evblack < 2500)
                {
                    deph = depth_base + 1;
                }
                else
                {
                    deph = depth_base;
                }

                player ^= true;
                //end of infinite loop

            }
        }



        public int ComputersMove(bool white, int totmoves,int depth_base)
        {

            CurrentHash = HashPosition(HashSeed);
            int deph = depth_base;



            uint mov = 0;
            uint[] mvm;
            bool end;
            bool player = white;

            int evwhite = LazyEvaluation(true);
            int evblack = LazyEvaluation(false);
            ulong f = 0;
            for (int m = 2; m < 12; ++m)
            {
                //check if there are other pieces than pawns
                if (m == 6 || m == 7)
                    continue;
                f |= BitBoards[m];
            }
            deph = depth_base;
            if (f == 0)
            {
                //only pawn endgame
                deph += 3;
            }
            else if (evwhite + evblack < 600)
            {
                deph += 4;
            }
            else if (evwhite + evblack < 1200)
            {
                deph += 2;
            }
            else if (evwhite + evblack < 2500)
            {
                deph += 1;
            }
            else
            {
            }
            TranspoTable = new Dictionary<uint, Hashentry>[deph];
            for (int k = 0; k < TranspoTable.Length; ++k)
            {
                TranspoTable[k] = new Dictionary<uint, Hashentry>();
            }
            var swplay = new Stopwatch();

            NodesSearched = 0;
            TimeSpent = 0;

            int ab = 0;
            LINE principalVariation = new LINE(deph /*+ (player ? 0 : -1)*/);
            for (int k = 0; k < TranspoTable.Length; ++k)
            {
                TranspoTable[k] = new Dictionary<uint, Hashentry>();
            }
            if (player || !player)
            {
                NodesSearched = 0;
                swplay.Start();

                // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, false); //no random
                ab = AlphaBeta_Rewritten(0, deph /*+ (player ? 0 : -1)*/, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, true); //both random
                                                                                                                                                // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, player ? true : false); //white random
                                                                                                                                                // ab = AlphaBeta(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, player ? false : true); //black random

                //ab = AlphaBeta_Rewritten(0, deph, int.MinValue + 10, int.MaxValue - 10, player, principalVariation, false);

                swplay.Stop();
                TimeSpent = swplay.ElapsedMilliseconds;
                swplay.Reset();


                end = true;
                for (int i = 0; i < 1; ++i)
                {
                    mov = principalVariation.argmove[0];
                }
                if (mov == 0)
                {
                    end = true;
                    return EndMate(player);
                }

                /*Console.WriteLine("{0:X}", CurrentHash);
                mvm = MakeMove(mov, player);
                Console.WriteLine("{0:X}", CurrentHash);
                UndoMove(mvm, player);
                Console.WriteLine("{0:X}", CurrentHash);
                mvm = MakeMove(mov, player);
                Console.WriteLine("{0:X}", CurrentHash);*/
                mvm = MakeMove(mov, player);
                if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                {
                    UndoMove(mvm, player);
                    end = true;
                }
                else
                {
                    end = false;
                }
            }
            else
            {
                Console.WriteLine("Tah");
                string am = Console.ReadLine();
                int from = am[0] - 'a' + 8 * (8 - (am[1] - 48));
                int to = am[2] - 'a' + 8 * (8 - (am[3] - 48));
                int piece = 0;
                for (int n = 0; n < 12; ++n)
                {
                    if (Bit(BitBoards[n], from))
                    {
                        piece = n;
                        break;
                    }


                }

                if (player)
                {
                    mov = BuildMoveWhite(piece, from, to);
                }
                else
                {
                    mov = BuildMoveBlack(piece, from, to);
                }
                mvm = MakeMove(mov, player);
                if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                {
                    UndoMove(mvm, player);
                    Console.WriteLine("Error: Illegal Move");
                    Console.WriteLine(DecodeMove((int)mov));
                    end = true;

                }
                else
                {
                    end = false;
                }
            }
            if (end)
            {
                return EndMate(player);
            }

            Notation += DecodeMove((int)mov) + " ";
            Notation += "{" + Evaluation().ToString() + "} ";

            var swr = new StreamWriter("partie.txt", false);
            swr.Write(Notation);
            swr.Close();

            if (InsufMaterial()||NoProgress())
            {
                EndDraw();
                return 1;
            }
            if (!player)
            {
                totmoves += 1;
            }
                

            player ^= true;
            //end of infinite loop
            return 0;
        }

        public int PlayersMove(bool white,int totmoves, uint mov)
        {

            CurrentHash = HashPosition(HashSeed);




            uint[] mvm;
            bool end;
            bool player = white;
            string writtentext = ""; //a text which will display in the text box
            int evwhite = LazyEvaluation(true);
            int evblack = LazyEvaluation(false);
            
            
            var swplay = new Stopwatch();
            TimeSpent = 0;


            if (false)
            {

            }
            /*if (player || !player)
            {
                swplay.Start();
                swplay.Stop();
                TimeSpent = swplay.ElapsedMilliseconds;
                swplay.Reset();


                end = true;

                Console.WriteLine("{0:X}", CurrentHash);
                mvm = MakeMove(mov, player);
                Console.WriteLine("{0:X}", CurrentHash);
                UndoMove(mvm, player);
                Console.WriteLine("{0:X}", CurrentHash);
                mvm = MakeMove(mov, player);
                Console.WriteLine("{0:X}", CurrentHash);
                mvm = MakeMove(mov, player);
                if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                {
                    UndoMove(mvm, player);
                    end = true;
                }
                else
                {
                    end = false;
                }
                end = false;
            }*/
            else
            {
                /*if (player)
                {
                    mov = BuildMoveWhite(piece, from, to);
                }
                else
                {
                    mov = BuildMoveBlack(piece, from, to);
                }*/
                mvm = MakeMove(mov, player);
                if (Attacked(player ? Wking : Bking, Wmask, Bmask, Block, !player, BitBoards))
                {
                    UndoMove(mvm, player);
                    //Console.WriteLine("Error: Illegal Move");
                    //Console.WriteLine(DecodeMove((int)mov));
                    writtentext = "Error: Illegal Move" + DecodeMove((int)mov);
                    //end = true;

                }
                else
                {
                    writtentext = DecodeMove((int)mov);
                    end = false;
                }
            }
            /*if (end)
            {
                return EndMate(player);
            }*/

            Notation += DecodeMove((int)mov) + " ";
            Notation += "{" + Evaluation().ToString() + "} ";

            var swr = new StreamWriter("partie.txt", false);
            swr.Write(Notation);
            swr.Close();

            if (InsufMaterial() || NoProgress())
            {
                EndDraw();
                return 1;
            }
            if (!player)
            {
                totmoves += 1;
            }


            player ^= true;
            //end of infinite loop
            return 0;
        }

        public uint CompleteMove(int piece, int from, int to, bool white)
        {
            if (white)
            {
                return BuildMoveWhite(piece, from, to);
            }
            else
            {
                return BuildMoveBlack(piece, from, to);
            }
        }

        public ulong DisplayLegalMoves(int idx, bool white) 
        {
            //generates legal moves for piece on this square, or pieces which can capture this piece
            ulong legalmoves = 0;
            ulong allmoves = 0;
            int piece = -1;

            for(int i = white ? 0 : 6; i < 6 + (white ? 0 : 6); ++i)
            {
                if (Bit(BitBoards[i], idx))
                {
                    piece = i;
                    break;
                }
            }
            if (piece == -1)
                return 0;

            if (white && piece < 6 || (white is false && piece >= 6))
            { //if the player is the same as the piece's color, generate moves for it
                allmoves = GenMoves(idx, piece, white, Wmask, Bmask, Block);

                switch (piece)
                {
                    case 1:
                        int epsq = (Position >> 8) & 0b111111;
                        if ((epsq != 0) && Bit(BitBoards[BB_p], epsq + 8))
                        {
                            int[] eps = EPGen(epsq, true);
                            foreach (var e in eps)
                            {//if the capture exists, the 7th bit must be set
                                if ((e & (1 << 7)) != 0)
                                    allmoves |= (ulong)1 << e;
                            }
                        }
                        break;
                    case 7:
                        epsq = (Position >> 8) & 0b111111;
                        if ((epsq != 0) && Bit(BitBoards[BB_P], epsq - 8))
                        {
                            int[] eps = EPGen(epsq, false);
                            foreach (var e in eps)
                            {//if the capture exists, the 7th bit must be set
                                if ((e & (1 << 7)) != 0)
                                    allmoves |= (ulong)1 << e;
                            }
                        }
                        break;
                    case 0:
                        byte c = Castling_White(Position);

                        //sets the 5th or 6th bit
                        if ((c & 1) != 0)
                        {
                            allmoves |= (ulong)1 << 62;
                        }
                        if ((c & 2) != 0)
                        {
                            allmoves |= (ulong)1 << 58;
                        }
                        break;
                    case 6:
                        c = Castling_Black(Position);

                        //sets the 5th or 6th bit
                        if ((c & 1) != 0)
                        {
                            allmoves |= (ulong)1 << 2;
                        }
                        if ((c & 2) != 0)
                        {
                            allmoves |= (ulong)1 << 6;
                        }
                        break;

                }
               

                ulong pointr = 1;
                for (int i = 0; i < 64; i++)
                {
                    if((allmoves & pointr) !=0) //pseudolegal square to be moved on 
                    {
                        uint pseudolegalmove = white ? BuildMoveWhite(piece, idx, i) : BuildMoveBlack(piece, idx, i);
                        uint[] backup = MakeMove(pseudolegalmove, white);
                        //if the move is legal = king is not attacked
                        if (!Attacked(white ? Wking : Bking, Wmask, Bmask, Block, !white, BitBoards))
                        {
                            legalmoves |= pointr;
                        }
                        //and returns back the position
                        UndoMove(backup, white);
                    }
                    pointr <<= 1;
                }
            }
            else
            { //if the player is the opposite as the piece's color, highlight all pieces that can capture it

            }
            return legalmoves;
        }


        public struct LINE
        {
            public int totalmoves;
            public uint[] argmove;
            public LINE(int t)
            {
                totalmoves = 0;
                argmove = new uint[t];
            }
        }

        public static int AlphaBeta(int currdepth, int maxdepth, int alpha, int beta, bool white, LINE pline, bool evaluation_system)
        {

            //var ss = new Stopwatch();
            //ss.Start();

            int curreval;
            LINE line = new LINE(maxdepth);
            bool end = true;
            if (currdepth >= maxdepth)
            {
                NodesSearched += 1;
                pline.totalmoves = 0;
                int ira = Quisce(int.MinValue + 10, int.MaxValue - 10, white, evaluation_system);

                /*ss.Stop();
                Console.WriteLine("{0}. {1}", NodesSearched, ss.Elapsed);
                ss.Reset();*/

                return ira;
                // return Evaluation() * (white ? 1 : -1);
            }

            /*TimeSpan g;
            TimeSpan gs;
            TimeSpan gss;*/


            //stops.Start();
            uint[] mvs = white ? MoveGeneration_White(BitBoards).ToArray() : MoveGeneration_Black(BitBoards).ToArray();
            //g = stops.Elapsed;
            //Shuffle(mvs);
            //gs = stops.Elapsed;
            SortMoves(mvs);
            //gss = stops.Elapsed;
            //Console.WriteLine("Elapsed generation: {0}", g);
            //Console.WriteLine("Elapsed generation+shuffling: {0}", gs);
            //Console.WriteLine("Elapsed generation+shuffling+sorting: {0}", gss);

            //stops.Reset();

            //Shuffling and sorting takes less than 45 % of time needed for generation, and is more useful

            NodesSearched += 1;
            for (int i = 0; i < mvs.Length; ++i)
            {
                //int checkito = Evaluation();
                uint donemove = mvs[i];
                uint[] mvm = MakeMove(donemove, white);

                //what if move is illegal
                if (Attacked(white ? Wking : Bking, Wmask, Bmask, Block, !white, BitBoards))
                {
                    UndoMove(mvm, white);
                    /*int ev2 = Evaluation();
                    if (checkito != ev2)
                    {
                        Console.WriteLine("Error in retracting");
                        Console.ReadLine();
                        Printout(BitBoards);
                        mvm = MakeMove(donemove, white);
                        Printout(BitBoards);
                        UndoMove(mvm, white);
                        Printout(BitBoards);
                    }*/
                    continue;
                }
                end = false;

                //ss.Stop();

                curreval = -AlphaBeta(currdepth + 1, maxdepth, -beta, -alpha, !white, line, evaluation_system);

                //ss.Start();

                UndoMove(mvm, white);
                /*int ev1 = Evaluation();
                if (checkito != ev1)
                {
                    Console.WriteLine("Error in retracting");
                    Console.ReadLine();
                    Printout(BitBoards);
                    mvm = MakeMove(donemove, white);
                    Printout(BitBoards);
                    UndoMove(mvm, white);
                    Printout(BitBoards);
                }*/

                if (curreval >= beta)
                {

                    /*ss.Stop();
                    Console.WriteLine("{0}. {1}", NodesSearched, ss.Elapsed);
                    ss.Reset();*/

                    return beta;
                }
                if (curreval > alpha)
                {

                    alpha = curreval;
                    //change principal variation

                    pline.argmove[0] = donemove;
                    //copy this:
                    /*
                        line.argmove, //which move we start
                        pline.argmove + 1, //max moves from current line, increased length
                        line.totalmoves * sizeof(uint) //how many of the integers 
                     */
                    //first element of pline stays, and the whole "line" gets added
                    int s = 0;
                    for (int l = 0; line.argmove[l] != 0; ++l)
                    {
                        s++;
                    }
                    Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                    //Array.Copy(pline.argmove, ref line.argmove +1, line.totalmoves+1);
                    pline.totalmoves = line.totalmoves + 1;
                }
            }
            if (end)
            { //if player has no legal moves, it's mate or stalemate
                /*ss.Stop();
                Console.WriteLine("{0}. {1}", NodesSearched, ss.Elapsed);
                ss.Reset();*/
                return -EndMateEval(white, 1 + currdepth);
            }
            if (InsufMaterial())
            {
                /*ss.Stop();
                Console.WriteLine("{0}. {1}", NodesSearched, ss.Elapsed);
                ss.Reset();*/
                return 0;
            }
            /*ss.Stop();
            Console.WriteLine("{0}. {1}", NodesSearched, ss.Elapsed);
            ss.Reset();*/
            return alpha;
        }

        static int Quisce(int alpha, int beta, bool white, bool evaluation_system) //,LINE pline)
        {
            int staticEval = 0;
            if (InsufMaterial())
            {
                staticEval = 0;
            }
            else
            {
                staticEval =
                evaluation_system ?
                (Evaluation() + los.Next(-25, +26))
                : Evaluation();

                staticEval *= (white ? 1 : -1);
            }
            
            //if there is no good capture, consider the score of position good as it is
            if (staticEval >= beta)
                return beta;
            if (alpha < staticEval)
            {
                alpha = staticEval;
                //add principal variation here
                /*pline.argmove[0] = emptymove;
                //if (pline.argmove[0] != 0 || line.argmove[0] != 0)
                {
                    int s = 0;
                    int p = 0;
                    for (int l = 0; l < line.argmove.Length; ++l)
                    {
                        if (line.argmove[l] == 0)
                        {
                            s = l;
                            break;
                        }
                    }
                    for (int l = 0; l < pline.argmove.Length; ++l)
                    {
                        if (pline.argmove[l] == 0)
                        {
                            p = l;
                            break;
                        }
                    }
                    //Array.Resize(ref pline.argmove, s + p+1);
                    Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                }*/

            }
            uint[] mvs = white ? CapsGeneration_White(BitBoards).ToArray() : CapsGeneration_Black(BitBoards).ToArray();

            int curreval;
            if (mvs.Length > 1)
            {

                //Shuffle(mvs);
                //give some ordering to the moves later, based on capturing and captured
                SortMoves(mvs);
                //make the moves as they appear in captures, same as in alphabeta

            }
            //NodesSearched += 1;
            for (int i = 0; i < mvs.Length; ++i)
            {

                //int checkito = Evaluation();
                uint donemove = mvs[i];
                uint[] mvm = MakeMove(donemove, white);

                //what if move is illegal
                if (Attacked(white ? Wking : Bking, Wmask, Bmask, Block, !white, BitBoards))
                {
                    UndoMove(mvm, white);
                    /*int ev2 = Evaluation();
                    if (checkito != ev2)
                    {
                        Console.WriteLine("Error in retracting");
                        Console.ReadLine();
                        Printout(BitBoards);
                        mvm = MakeMove(donemove, white);
                        Printout(BitBoards);
                        UndoMove(mvm, white);
                        Printout(BitBoards);
                    }*/
                    continue;
                }


                curreval = -Quisce(-beta, -alpha, !white, evaluation_system);
                UndoMove(mvm, white);


                /*int ev1 = Evaluation();
                if (checkito != ev1)
                {
                    Console.WriteLine("Error in retracting");
                    Console.ReadLine();
                    Printout(BitBoards);
                    mvm = MakeMove(donemove, white);
                    Printout(BitBoards);
                    UndoMove(mvm, white);
                    Printout(BitBoards);
                }*/

                if (curreval >= beta)
                {
                    return beta;
                }
                if (curreval > alpha)
                {
                    alpha = curreval;
                    //increase the PV 
                    /*pline.argmove[0] = donemove;
                    int s = 0;
                    int p = 0;
                    for (int l = 0; l < line.argmove.Length; ++l)
                    {
                        if (line.argmove[l] == 0)
                        {
                            s = l;
                            break;
                        }
                    }
                    for (int l = 0; l < pline.argmove.Length; ++l)
                    {
                        if (pline.argmove[l] == 0)
                        {
                            p = l;
                            break;
                        }
                    }
                    //Array.Resize(ref pline.argmove, s + p+1);
                    Array.Copy(line.argmove, 0, pline.argmove, 1, s);
                    //Array.Copy(pline.argmove, ref line.argmove +1, line.totalmoves+1);
                    pline.totalmoves = line.totalmoves + 1;
                    */
                }
            }

            return alpha;
        }
        static string[] databaze = new string[]
            {
            "r1bqkbnr/pppp1ppp/2n5/4p3/3PP3/5N2/PPP2PPP/RNBQKB1R/ KQkq b --", //skotska
            "3r3r/5pk1/3Rp1p1/p3p1P1/1p2P3/6Q1/PPP1qP2/1K2N1R1/ ---- b --", //taktika - cerny
            "7k/1p1b1Bb1/3p3p/1Pq1p3/4n3/5NQP/3B1PP1/6K1/ ---- w -- ", //taktika - bily
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR/ KQkq w --", //zakladni 
            "8/8/8/4k3/8/8/RK6/1R6/ ---- w --", //veze
            "8/k7/2K5/1P6/8/8/8/8/ ---- w --", //kral pesec
            "r3k2r/8/8/6Pp/1p6/8/P5p1/R3K2R/ KQkq w h6", //rosady test
            "k7/8/6RR/8/8/8/8/2K5/ ---- w --", //rychly mat
            "7k/8/8/8/8/8/8/K7/ ---- w --", //jen kralove
            "k3b3/1p1p1p1p/1P1P1P1P/8/8/1p1p1p1p/1P1P1P1P/K3B3/ ---- w --", //kompletni blokada
            "8/3q4/1K3k2/r7/6R1/8/8/8/ ---- b --", //nenalezeny mat za cerneho
            "8/8/8/2p5/2P5/5p2/2K2k2/RQQQqrr1/  KQkq w --", //brani
            "4Q3/2b3pk/P6p/7P/4p3/8/5P1q/5RK1/ ---- w --", //mat nebo pat
            "KR6/8/5k2/7p/8/8/8/8/ ---- w --", //vez proti pesci (musis zabranit opakovani)
            };
        public ulong[] Initialize(int idvychozi)
        {
            for (int j = 0; j < 1; ++j)
            {
                string pozice = databaze[idvychozi];
                //byte rosad = rosady[pz - 1];
                char[] ka = TightFenToChar(FenToStr(pozice));
                bool zacina = !((Position & (1 << 4)) == 0);
                Wmask = 0;
                Bmask = 0;
                BitBoards = CreateBBoards(ka);
                int[] AllPieces = CreatePieces(ka);
                for (int i = 0; i < 64; ++i)
                {
                    if (ka[i] != '\0')
                    {
                        switch (ka[i])
                        {
                            case 'p':
                            case 'k':
                            case 'n':
                            case 'b':
                            case 'r':
                            case 'q':
                                Bmask |= (((ulong)(1)) << i);
                                break;
                            case 'P':
                            case 'K':
                            case 'N':
                            case 'B':
                            case 'R':
                            case 'Q':
                                Wmask |= (((ulong)(1)) << i);
                                break;
                        }
                    }
                }
                Remask();
                //Block = Wmask | Bmask;

                //Printout(BitBoards);
                ulong h = HashPosition(HashSeed);
                //Console.WriteLine("{0:X}", h);
                //Play(zacina);
                //return;

            }
            return BitBoards;
        }
        public static void Start()
        {
            
            for (int j = 0; j < 1; ++j)
            {
                //Console.WriteLine("Vyber si pozici");
                int pozid = int.Parse(Console.ReadLine());
                //int pozid = j;
                string pozice = databaze[pozid];
                //byte rosad = rosady[pz - 1];
                char[] ka = TightFenToChar(FenToStr(pozice));
                bool zacina = !((Position & (1 << 4)) == 0);
                Wmask = 0;
                Bmask = 0;
                BitBoards = CreateBBoards(ka);
                int[] AllPieces = CreatePieces(ka);
                for (int i = 0; i < 64; ++i)
                {
                    if (ka[i] != '\0')
                    {
                        switch (ka[i])
                        {
                            case 'p':
                            case 'k':
                            case 'n':
                            case 'b':
                            case 'r':
                            case 'q':
                                Bmask |= (((ulong)(1)) << i);
                                break;
                            case 'P':
                            case 'K':
                            case 'N':
                            case 'B':
                            case 'R':
                            case 'Q':
                                Wmask |= (((ulong)(1)) << i);
                                break;
                        }
                    }
                }
                Remask();
                //Block = Wmask | Bmask;

                //Printout(BitBoards);
                List<uint> mvsh;

                /*mvsh= MoveGeneration_White(BitBoards);
                foreach (int move in mvsh)
                {
                    Console.WriteLine(DecodeMove(move));
                }
                mvsh = MoveGeneration_Black(BitBoards);
                foreach (int move in mvsh)
                {
                    Console.WriteLine(DecodeMove(move));
                }
                for(int i = 0; i < 64; ++i)
                {
                    string sq = Squares[i];
                    Console.WriteLine("{0} - {1},{2}", sq, Attacked(i, Wmask, Bmask, Block, true, BitBoards), Attacked(i, Wmask, Bmask, Block, false, BitBoards));
                }*/
                var los = new Random();
                uint mov = 0;

                string nota = "";

                uint[] mvs;
                uint[] mvm;
                /*if (zacina)
                {
                    mvs = MoveGeneration_White(BitBoards).ToArray();
                    Shuffle(mvs);
                    for (int i = 0;i<=mvs.Length;++i)
                    {
                        mov = mvs[i];
                        //mov = mvs[3];
                        mvm = MakeMove(mov, true);
                        if (Attacked(Wking, Wmask, Bmask, Block, false, BitBoards))
                        {
                            UndoMove(mvm, true);
                        }
                        else
                            break;
                    }
                    Printout(BitBoards);
                    Console.WriteLine(DecodeMove((int)mov));
                    nota += DecodeMove((int)mov) + " ";
                    var swr = new StreamWriter("partie.txt",false);
                    swr.Write(nota);
                    swr.Close();
                }*/

                ulong h = HashPosition(HashSeed);
                //Console.WriteLine("{0:X}", h);
                Play(zacina);
                //return;

            }
            return ;
        }
    }
    public class ULongRandom
    {
        private static byte[] buffer = new byte[sizeof(ulong)];
        private static Random random = new Random();

        public ULongRandom(int seed)
        {
            buffer = new byte[sizeof(ulong)];
            random = new Random(seed);
        }
        public ulong Next()
        {
            random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
