﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Neuron
    {
        // neuron's number
        public long NeurNum { get; private set; }

        // index in input vector (index of pixel occupied by the neuron)
        public int NeurIndInput { get; set; }

        // neuron's position
        public float Neur_X { get; set; }
        public float Neur_Y { get; set; }

        // the distance needed to move the node after processing 
        public float Delta { get; set; }

        // position index in _Position list
        private int PosInd { get; set; }

        /* // output axon connection to the id
         public int Next_num { get => next_num; set => next_num = value; }

         // output axon connection weight
         public float Next_weight { get => next_weight; set => next_weight = value; } */

        // output axons
        public List<NumWeights> Axons { get; set; } = new List<NumWeights>();

        // synapses (inputs)
        public List<NumWeights> Synapses { get; set; } = new List<NumWeights>();

        // previous experience
        public List<PrevExp> Experience { get; set; } = new List<PrevExp>();

        // error
        public int Error { get; set; }

        // local error (sum of errors) 
        public int E { get; set; }

        // bool value representing whether distance has been processed or not
        bool Processed { get; set; }

        // check direction of search for ProcessDistance method
        public bool Forward { get; set; }

        // location of node compared to MainInput vector (Left == true => Increase Delta value in Adapt_weights function)
        public bool Left { get; set; }

        private static Random _rnd = new Random();

        public Neuron(long num)
        {
            NeurNum = num;
            Error = 0;
            NeurIndInput = -1;
            Processed = false;
        }

        public Neuron(long num, float _x, float _y)
        {
            // initialization with default values except for neuron's number
            NeurNum = num;
            Error = 0;
            NeurIndInput = -1;
            Processed = false;

            // making limit for neuron's position so it wouldn't be possible for it to get out of the picture's borders
            if (((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) < _x)
            {
                _x = ((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.K);
            }

            if ((((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw) > _x)
            {
                _x = (((MainWindow.Vpw - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw);
            }

            if (((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) < _y)
            {
                _y = ((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.K);
            }

            if ((((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph) > _y)
            {
                _y = (((MainWindow.Vph - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph);
            }

            // applying constructor's parameters to neuron's coordinates and coordinates in visualisation
            MainWindow.X_visual = Neur_X = _x;
            MainWindow.Y_visual = Neur_Y = _y;

            // drawing triangle that represents one node:
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.0f * MainWindow.K) + MainWindow.X_visual),
               ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.5f * MainWindow.K) + MainWindow.X_visual),
             ((1.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((1.0f * MainWindow.K) + MainWindow.X_visual),
             ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));

            // remembering index of the first vertice of the triangle
            PosInd = MainWindow._Position.Count - 3;

            // calculating neuron's index value based on neuron's X and Y coordinates used in visualisation
            NeurIndInput = MainWindow.CalculateIndex(Neur_X, Neur_Y);

            MainWindow._TriangleEdges.Add((ushort)(num + (2 * num) + 0));
            MainWindow._TriangleEdges.Add((ushort)(num + (2 * num) + 1));
            MainWindow._TriangleEdges.Add((ushort)(num + (2 * num) + 2));

            // adding purple color to the triangle which is representing one node
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);

            MainWindow.debug1 = MainWindow.MainInput[4 * NeurIndInput + 2];
            MainWindow.debug2 = MainWindow.MainInput[4 * NeurIndInput + 1];
            MainWindow.debug3 = MainWindow.MainInput[4 * NeurIndInput];
            MainWindow.debug4 = NeurIndInput;
        }

        ~Neuron() { }

        // neuron's comparison algorithm       // // // // // // // // WIP
        private void Compare()
        {



        }

        // change visualisation if current location has changed           // // // // // // // // // // // // // // // /WIP
        public void ChangePosition()
        {
            MainWindow.X_visual = Neur_X;
            MainWindow.Y_visual = Neur_Y;

            MainWindow._Position[PosInd] = new OpenGL.Vertex3f(((0.0f * MainWindow.K) + MainWindow.X_visual),
               ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f);
            MainWindow._Position[PosInd + 1] = new OpenGL.Vertex3f(((0.5f * MainWindow.K) + MainWindow.X_visual),
             ((1.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f);
            MainWindow._Position[PosInd + 2] = new OpenGL.Vertex3f(((1.0f * MainWindow.K) + MainWindow.X_visual),
             ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f);

            // System.Threading.Thread.Sleep(1000);
            // Gl.Clear(ClearBufferMask.ColorBufferBit);
            // Gl.BindVertexArray(MainWindow._TriangleVao);

            // updating gl buffers in order to redraw nodes' positions
            MainWindow.Change_pos = true;
        }

        // for the first winner find the nearest value in MainInput vector and update local error 
        public void ProcessDistance() ///////////Work In Progress/////////// 
        {
            // checking whether the node has the index value
            if (NeurIndInput != (-1))
            {
                // check the direction of search
                if (Forward == true)
                { 
                    // changing direction of the search for backward search in separate thread
                    Forward = false;
                    for (int i = NeurIndInput; i < MainWindow.MainInput.Length; i++)
                    {
                        // searching for the first pixel that is different from black background
                        if ((Processed == false) && ((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[4 * i + 1] > 0) || (MainWindow.MainInput[4 * i + 2] > 0)))
                        {
                            MainWindow.debug5 = MainWindow.MainInput[4 * i + 2];
                            MainWindow.debug6 = MainWindow.MainInput[4 * i + 1];
                            MainWindow.debug7 = MainWindow.MainInput[4 * i];
                            MainWindow.debug8 = i;
                            Error += (int)Math.Pow(Math.Abs((i - NeurIndInput) * MainWindow._pixelSize), 2);

                            // changing Delta (distance) for further change of winner's position and it's synapses values as well as neighbours position and their axons values
                            Delta = NeuralNetwork.Eps_w * ((i - NeurIndInput) * MainWindow._pixelSize);
                            // activating trigger value to stop multithread search
                            Processed = true;
                            // set location value in relation to MainInput vector for further use in the Adapt_weights function
                            Left = true;
                            break;
                        }
                        Thread.Sleep(0);
                    }
                }
                else
                {
                    for (int i = NeurIndInput; i > 0; i--)
                    {
                        if ((Processed == false) && ((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[4 * i + 1] > 0) || (MainWindow.MainInput[4 * i + 2] > 0)))
                        {
                            MainWindow.debug5 = MainWindow.MainInput[4 * i + 2];
                            MainWindow.debug6 = MainWindow.MainInput[4 * i + 1];
                            MainWindow.debug7 = MainWindow.MainInput[4 * i];
                            MainWindow.debug8 = i;
                            Error += (int)Math.Pow(Math.Abs((NeurIndInput - i) * MainWindow._pixelSize), 2);
                            Delta = NeuralNetwork.Eps_w * ((NeurIndInput - i) * MainWindow._pixelSize);
                            Processed = true;
                            Left = false;
                            break;
                        }
                        Thread.Sleep(0);
                    }
                }
                
            }
            else
            {
                // WIP add exception or warning message
            }
        }

    } // -->Neuron

    public struct NumWeights // neuron's connections (synapses, axons) id_of_weight/weight_amount(X,Y)
    {
        public NumWeights(int weight_id, long neigh_id, float weightX, float weightY)
        {
            NeighId = neigh_id;
            WeightDataId = weight_id;
            WeightDataX = weightX;
            WeightDataY = weightY;
        }

        // input's weight id that is used in the linked list of 'weight ids' (starting from strongest weight to the weakest) 
        public int WeightDataId { get; private set; }         // // // // // // // // // // // // // // /

        // the amount of weight of each synapse/axon
        public float WeightDataX { get; set; }
        public float WeightDataY { get; set; }

        // the id of neighbour in connection
        public long NeighId { get; set; }
    }

    public struct PrevExp // neuron's experience                  // ~~~~~~~~~~~~?
    {
        public PrevExp(int x, int y)
        {
            Row = x;
            Column = y;
        }

        // Previous experience matrix coordinates x
        public int Row { get; private set; }

        // Previous experience matrix coordinates y
        public int Column { get; private set; }
    }
}