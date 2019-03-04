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
        // neuron's id
        public long NeurId { get; private set; }

        // neuron's index in the input vector (the index of the pixel occupied by the neuron)
        public int NeurIndInInput { get; set; }

        // neuron's position in visualisation (from 0 to 1.f)
        public float Neur_X { get; set; }
        public float Neur_Y { get; set; }

        // the distance needed to move the node after the distance processing 
        public float Delta { get; set; }

        // position index in _Position list
        private int PosInd { get; set; }

        // nearest value to the left from the node
        public int DistL { get; set; }

        public int DistR { get; set; }

        /* // output axon connection to the id
         public int Next_num { get => next_num; set => next_num = value; }

        // output axon connection weight
        public float Next_weight { get => next_weight; set => next_weight = value; } */

        // output axons
        public List<ConnWeights> AxonsWeights { get; set; } = new List<ConnWeights>();

        // synapses (inputs)
        public List<ConnWeights> SynapsesWeights { get; set; } = new List<ConnWeights>();

        // previous experience
        public List<PrevExp> Experience { get; set; } = new List<PrevExp>();

        // error
        public float Error { get; set; }

        // local error (sum of all errors that occured)
        public float E { get; set; }                
        
        // check direction of search for ProcessDistance method
        public bool Forward { get; set; }

        // location of node compared to MainInput vector (Left == true => Increase Delta value in Adapt_weights function)
        public bool Left { get; set; }

        // index of the right border pixel in relation to the current node  
        public int RightBorder { get; set; }

        public int LeftBorder { get; set; }
        
        private static Random _rnd = new Random();

        public Neuron(long id)
        {
            NeurId = id;
            Error = 0;
            NeurIndInInput = -1;
        }

        public Neuron(long id, float _x, float _y)
        {
            // initialization with default values except for neuron's number
            NeurId = id;
            Error = 0;
            NeurIndInInput = -1;

            // limit neuron's position so it wouldn't be possible for it to get out of the picture's borders
            if (((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) < _x)
            {
                _x = ((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.NodeScale);
            }

            if ((((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw) > _x)
            {
                _x = (((MainWindow.Vpw - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw);
            }

            if (((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) < _y)
            {
                _y = ((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.NodeScale);
            }

            if ((((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph) > _y)
            {
                _y = (((MainWindow.Vph - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph);
            }

            // applying constructor's parameters to neuron's coordinates in visualisation
            MainWindow.X_visual = Neur_X = _x;
            MainWindow.Y_visual = Neur_Y = _y;

            // drawing triangle that represents one node:
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.0f * MainWindow.NodeScale) + MainWindow.X_visual),
               ((0.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.5f * MainWindow.NodeScale) + MainWindow.X_visual),
             ((1.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((1.0f * MainWindow.NodeScale) + MainWindow.X_visual),
             ((0.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f));

            // remembering index of the first vertice of the triangle
            PosInd = MainWindow._Position.Count - 3;

            // calculating neuron's index value based on neuron's X and Y coordinates in visualisation
            NeurIndInInput = MainWindow.CalculateIndex(Neur_X, Neur_Y);

            MainWindow._TriangleEdges.Add((ushort)(id + (2 * id) + 0));
            MainWindow._TriangleEdges.Add((ushort)(id + (2 * id) + 1));
            MainWindow._TriangleEdges.Add((ushort)(id + (2 * id) + 2));

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

            MainWindow.debug1 = MainWindow.MainInput[4 * NeurIndInInput + 2];
            MainWindow.debug2 = MainWindow.MainInput[4 * NeurIndInInput + 1];
            MainWindow.debug3 = MainWindow.MainInput[4 * NeurIndInInput];
            MainWindow.debug4 = NeurIndInInput;

            // getting the amount of rows by dividing the NeurIndInput value by the _image.Width value
            // rounding the rows value to farthest ten in order to acquire the right border value in the main input vector
            RightBorder = ((int)(NeurIndInInput / MainWindow._image.Width) + 1) * MainWindow._image.Width;
            // rounding the rows value to nearest ten in order to acquire the left border value in the main input vector
            LeftBorder = ((int)(NeurIndInInput / MainWindow._image.Width)) * MainWindow._image.Width;

        } //-->Neuron

        ~Neuron() { }

        // neuron's comparison algorithm       // WIP
        private void Compare()
        {



        }

        // change visualisation if current location has changed           // WIP
        public void ChangePosition()
        {
            // check whether coordinates located out of the borders (reduce the values accordingly)
            if (MainWindow._image.Height >= MainWindow._image.Width)
            {
                if (Neur_X < (MainWindow._frame / MainWindow.Vpw))
                {
                   Neur_X = (MainWindow._frame / MainWindow.Vpw); 
                }
                else if (Neur_X > ((MainWindow.Vpw - MainWindow._frame) / MainWindow.Vpw))
                {
                    Neur_X = ((MainWindow.Vpw - MainWindow._frame) / MainWindow.Vpw) - MainWindow.NodeScale; // "- NodeScale" means the width of the node
                }
            }
            else
            {
                if (Neur_Y < (MainWindow._frame / MainWindow.Vph))
                {
                    Neur_Y = (MainWindow._frame / MainWindow.Vph);
                }
                else if (Neur_Y > ((MainWindow.Vph - MainWindow._frame) / MainWindow.Vph))
                {
                    Neur_Y = ((MainWindow.Vph - MainWindow._frame) / MainWindow.Vph) - MainWindow.NodeScale;
                }
            }

            // checking whether neuron's position is out of the entire frame
            if (Neur_X > 1.0f)
            {
                Neur_X = 1.0f - MainWindow.NodeScale;
            }
            else if (Neur_X < 0)
            {
                Neur_X = 0;
            }
            else if (Neur_Y > 1.0f)
            {
                Neur_Y = 1.0f - MainWindow.NodeScale;
            }
            else if (Neur_Y < 0)
            {
                Neur_Y = 0;
            }

            MainWindow.X_visual = Neur_X;
            MainWindow.Y_visual = Neur_Y;

            MainWindow._Position[PosInd] = new OpenGL.Vertex3f(((0.0f * MainWindow.NodeScale) + MainWindow.X_visual),
               ((0.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f);
            MainWindow._Position[PosInd + 1] = new OpenGL.Vertex3f(((0.5f * MainWindow.NodeScale) + MainWindow.X_visual),
             ((1.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f);
            MainWindow._Position[PosInd + 2] = new OpenGL.Vertex3f(((1.0f * MainWindow.NodeScale) + MainWindow.X_visual),
             ((0.0f * MainWindow.NodeScale) + MainWindow.Y_visual), 0.0f);

            // System.Threading.Thread.Sleep(1000);
            // Gl.Clear(ClearBufferMask.ColorBufferBit);
            // Gl.BindVertexArray(MainWindow._TriangleVao);

            // updating gl buffers in order to redraw nodes' positions
            MainWindow.Change_pos = true;
        } //-->ChangePosition
        
        // Finds weight of the node if there's a change in pixel color to the right of the node. Used in multithread search of first occurence of pixel color change.
        public void ForwardSearch()
        {
            // checking whether the node has the index value
            if ((NeurIndInInput != (-1)) )
            {
                //for (int i = NeurIndInput; i < ((MainWindow.MainInput.Length / 4) - 2); i++)
                for (int i = NeurIndInInput; i < (RightBorder - 2); i++)
                {
                    // searching for the first pixel that is different from black background
                    if (((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[(4 * i) + 1] > 0) || (MainWindow.MainInput[(4 * i) + 2] > 0)))
                    {
                        DistR = i;
                        break;
                    }
                    // if there were no matches
                    DistR = -1;
                }
            }
        }
        
        // Finds weight of the node if there's a change in pixel color to the left of the node. Used in multithread search of first occurence of pixel color change.
        public void BackwardSearch()
        {
            // checking whether the node has the index value
            if ((NeurIndInInput != (-1)))
            {
                for (int i = NeurIndInInput; i > (LeftBorder - 2); i--)
                {
                    if (((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[(4 * i) + 1] > 0) || (MainWindow.MainInput[(4 * i) + 2] > 0)))
                    {
                        DistL = i;
                        break;
                    }
                    // if there were no matches
                    DistL = -1;
                }
            }
        }

        // Checks the distance between the node and the input vector to find the shortest distance where the value is located.
        public void CompareDistances()
        {
            if ((DistR != (-1)) && (DistL != (-1)))
            {
                // Comparing two values representing the distance from the node towards the input vector        
                if ((DistR - NeurIndInInput) < (NeurIndInInput - DistL))
                {
                    E += (int)Math.Pow(Math.Abs((DistR - NeurIndInInput) * MainWindow._pixelSize), 2);
                    // Changing Delta (distance) in order to move winner and it's synapses as well as it's neighbours and their axons
                    Delta = NeuralNetwork.Eps_w * ((DistR - NeurIndInInput));
                    // The Input vector value is located to the right from the node.
                    Left = false;
                }
                else if ((DistR - NeurIndInInput) > (NeurIndInInput - DistL)) // Left value is lower   
                {
                    E += (int)Math.Pow(Math.Abs((NeurIndInInput - DistL) * MainWindow._pixelSize), 2);
                    Delta = NeuralNetwork.Eps_w * ((NeurIndInInput - DistL));
                    Left = true;
                }
            }
            else if (DistR != (-1))
            {
                E += (int)Math.Pow(Math.Abs((DistR - NeurIndInInput) * MainWindow._pixelSize), 2);
                Delta = NeuralNetwork.Eps_w * ((DistR - NeurIndInInput));
                Left = false;
            }
            else if (DistL != (-1))
            {
                E += (int)Math.Pow(Math.Abs((NeurIndInInput - DistL) * MainWindow._pixelSize), 2);
                Delta = NeuralNetwork.Eps_w * ((NeurIndInInput - DistL));
                Left = true;
            }
            else
            {
                // if there were no matches in both directions

            }
        } //-->CompareDistances

    } //-->Neuron

    public struct ConnWeights // neuron's connections' (synapses, axons) id_of_weight/weight_amount(X,Y)
    {
        // connection id
        public int ConnId { get; set; }

        // the id of the neighbour in the connection (depends on whether it's axon (output) or synapse (input))
        public long NeighId { get; set; }

        // input's weight id that is used in the linked list of 'weight ids' (starting from the strongest weight to the weakest) 
        public int WeightDataId { get; private set; }      /////

        // the amount of weight of each synapse/axon  //////////
        public float WeightDataX { get; set; }
        public float WeightDataY { get; set; }

        public ConnWeights(int conn_id, int weight_id, long neigh_id, float weightX, float weightY)
        {
            ConnId = conn_id;
            WeightDataId = weight_id;
            NeighId = neigh_id;
            WeightDataX = weightX;
            WeightDataY = weightY;
        }

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
