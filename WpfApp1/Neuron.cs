using System;
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
        public int NeurInInputIndex { get; set; }

        // neuron's position in visualisation (from 0 to 1.f)
        public float Neur_X { get; set; }
        public float Neur_Y { get; set; }

        // the distance needed to move the node after the distance processing 
        public float Delta { get; set; }

        // position index in _Position list
        private int PosInd { get; set; }

        // nearest value to the left from the node
        public int DistL { get; set; } = -1;

        public int DistR { get; set; } = -1;

        // nearest value to the top from the node
        public int DistT { get; set; } = -1;

        // nearest value to the bottom from the node
        public int DistB { get; set; } = -1;

        // minimal distance for the winner neuron that will be used for the next winner search
        public int MinDist { get; set; } = -1;

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

        // Current error
        public float Error { get; set; }

        // local error (sum of all occured errors)
        public float E { get; set; }                
        
        // check direction of search for ProcessDistance method
        public bool Forward { get; set; }

        // location of node compared to MainInput vector (LeftNode == true => Increase Delta value in Adapt_weights function)
        public bool LeftNode { get; set; }
        public bool TopNode { get; set; }
        public bool BottomNode { get; set; }

        // thread lock for use in multithread search
        private static object locker = new object();

        // index of the right border pixel in relation to the current node  
      //  public int RightLimit { get; set; }

      //  public int LeftLimit { get; set; }
        
        private static Random _rnd = new Random();

        public Neuron(long id)
        {
            NeurId = id;
            Error = 0;
            NeurInInputIndex = -1;
        }

        public Neuron(long id, float _x, float _y)
        {
            // initialization with default values except for neuron's number
            NeurId = id;
            Error = 0;
            NeurInInputIndex = -1;

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
            NeurInInputIndex = MainWindow.CalculateIndex(Neur_X, Neur_Y);

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

            MainWindow.debug1 = MainWindow.MainInput[4 * NeurInInputIndex + 2];
            MainWindow.debug2 = MainWindow.MainInput[4 * NeurInInputIndex + 1];
            MainWindow.debug3 = MainWindow.MainInput[4 * NeurInInputIndex];
            MainWindow.debug_index = NeurInInputIndex;

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

            /*
            // Getting the amount of rows by dividing the NeurInInputIndex value by the _image.Width value.
            // Rounding the rows value to farthest ten in order to acquire the right border value in the main input vector
            // (a specific search limit in relation to the neuron).
            RightLimit = (((int)(NeurInInputIndex / MainWindow._image.Width) + 1) * MainWindow._image.Width) + 1;
            // rounding the rows value to nearest ten in order to acquire the left border value in the main input vector
            LeftLimit = ((int)(NeurInInputIndex / MainWindow._image.Width)) * MainWindow._image.Width;
            */
            
            // Updating gl buffers in order to redraw nodes' positions
            MainWindow.Change_pos = true;
        } //-->ChangePosition
        
        // Finds weight of the node if there's a change in pixel color to the right of the node. Used in the multithread search of first occurence of pixel color change.
        // Returns the amount of rows from the initial node where pixel color change took place.
        public void ForwardSearch()
        {
            // checking whether the node has the index value
            if ((NeurInInputIndex != (-1)))
            {
                // if there were no matches
                DistR = -1;
                // Initializing length of the first row (minus first cell).
                int RowLength = 2;

                lock (locker)
                {
                    // Creating up limit for the triangular search area.
                    int UpLimit = (NeurInInputIndex - MainWindow._image.Width + 1);
                    
                    // Getting the amount of rows by dividing the NeurInInputIndex value by the _image.Width value.
                    // Rounding the rows value to farthest ten in order to acquire the right border value in the main input vector
                    // (a specific search limit in relation to the neuron).
                    int RightLimit = (((int)(NeurInInputIndex / MainWindow._image.Width) + 1) * MainWindow._image.Width) + 1;

                    // Lowering RightLimit by row width to correlate with cell position
                    RightLimit += MainWindow._image.Width;
                    // Choosing cell located to the lower-right from the cell occupied by the neuron.
                    // Using search in each cell of the triangular shaped area.
                    for (int Cell = (NeurInInputIndex + MainWindow._image.Width + 1); Cell < RightLimit; Cell -= MainWindow._image.Width, RightLimit -= MainWindow._image.Width)
                    {
                        if (((Cell * 4) + 2) < (MainWindow.MainInput.Length - 1))
                        {
                            if ((Cell >= 0) && ((Cell * 4) >= 0))
                            {
                                if (((MainWindow.MainInput[4 * Cell] > 0) || (MainWindow.MainInput[(4 * Cell) + 1] > 0) || (MainWindow.MainInput[(4 * Cell) + 2] > 0)))
                                {
                                    DistR = Cell;
                                    return;
                                }

                                // Move cell to the bottom-right from it's row if it's location reached up limit.                   
                                if ((Cell == UpLimit) || (Cell < 0))
                                {
                                    Cell = (Cell + MainWindow._image.Width * RowLength) + MainWindow._image.Width + 1;
                                    RightLimit += (MainWindow._image.Width * RowLength) + MainWindow._image.Width;
                                    UpLimit = UpLimit + MainWindow._image.Width + 1;
                                    RowLength += 2;
                                }
                            }
                        }
                    }
                }
            }
        } //-->ForwardSearch()

        // Finds weight of the node if there's a change in pixel color to the left of the node. Used in the multithread search of first occurence of pixel color change.
        // Returns the amount of rows from the initial node where pixel color change took place.
        public void BackwardSearch()
        {
            // checking whether the node has the index value
            if ((NeurInInputIndex != (-1)))
            {
                // if there were no matches
                DistL = -1;
                // Initializing length of the first row (minus first cell).
                int RowLength = 2;

                lock (locker)
                {
                    // Creating up limit for the triangular search area.
                    int UpLimit = (NeurInInputIndex - MainWindow._image.Width - 1);
                    
                    // Getting the amount of rows by dividing the NeurInInputIndex value by the _image.Width value.
                    // rounding the rows value to nearest ten in order to acquire the left border value in the main input vector
                    int LeftLimit = ((int)(NeurInInputIndex / MainWindow._image.Width)) * MainWindow._image.Width;

                    // Lowering LeftLimit by row width to correlate with cell position
                    LeftLimit += MainWindow._image.Width;
                    // Choosing cell located to the lower-left from the cell occupied by the neuron.
                    // Using search in each cell of the triangular shaped area.
                    for (int Cell = (NeurInInputIndex + MainWindow._image.Width - 1); Cell > LeftLimit; Cell -= MainWindow._image.Width, LeftLimit -= MainWindow._image.Width)
                    {
                        if (((Cell * 4) + 2) < (MainWindow.MainInput.Length - 1))
                        {
                            if ((Cell >= 0) && ((Cell * 4) >= 0))
                            {
                                if (((MainWindow.MainInput[4 * Cell] > 0) || (MainWindow.MainInput[(4 * Cell) + 1] > 0) || (MainWindow.MainInput[(4 * Cell) + 2] > 0)))
                                {
                                    DistL = Cell;
                                    return;
                                }

                                // Move cell to the bottom-left from it's row if it's location reached up limit.                   
                                if (Cell == UpLimit)
                                {
                                    Cell = (Cell + MainWindow._image.Width * RowLength) + MainWindow._image.Width - 1;
                                    LeftLimit += (MainWindow._image.Width * RowLength) + MainWindow._image.Width;
                                    UpLimit = UpLimit + MainWindow._image.Width - 1;
                                    RowLength += 2;
                                }
                            }
                        }
                    }
                }
            }
        } //-->BackwardSearch()

        // Looks for the first pixel occurence to the top from the node in the multithread search (search through the row from left to right, then increase the row)
        public void TopSearch()
        {
            // Calculating row width based on the amount of columns
            //float RowWidth = MainWindow._image.Width * MainWindow._pixelSize; 

            // Checking whether the node has the index value
            if (NeurInInputIndex != (-1))
            {
                // If there were no matches throughout search.
                DistT = -1;
                // Initializing length of the first row (minus first cell).
                int RowLength = 2;

                lock (locker)
                {
                    // Creating right limit for the triangular search area.
                    // RightLimit for the TopSearch differs from that of the ForwardSearch. Here it symbolizes the end of each row of the triangular search area.
                    int RightLimit = (NeurInInputIndex - MainWindow._image.Width - 1) + RowLength;
                    // Lowering RightLimit by row width to correlate with cell position
                    RightLimit += MainWindow._image.Width;
                                        
                    // Choosing cell located to the upper-left from the cell occupied by the neuron.
                    // Using search in each cell of the triangular shaped area.
                    for (int Cell = (NeurInInputIndex - MainWindow._image.Width - 1); Cell > 0; Cell++)
                    {
                        if ((Cell >= 0) && ((Cell * 4) >= 0))
                        {
                            if (((Cell * 4) + 2) < (MainWindow.MainInput.Length - 1))
                            {
                                // Quit search if first pixel occurence took place.
                                if ((MainWindow.MainInput[4 * Cell] > 0) || (MainWindow.MainInput[(4 * Cell) + 1] > 0) || (MainWindow.MainInput[(4 * Cell) + 2] > 0))
                                {
                                    DistT = Cell;
                                    return;
                                }

                                // Move cell to the upper-left from it's row if it's location reached right limit.                   
                                if (Cell == RightLimit)
                                {
                                    Cell = (Cell - RowLength) - MainWindow._image.Width - 1;
                                    RightLimit = RightLimit - MainWindow._image.Width + 1;
                                    RowLength += 2;
                                }
                            }
                        }
                    }
                }
            }
        } //-->TopSearch()

        public void BottomSearch()
        {
            // Checking whether the node has the index value
            if ((NeurInInputIndex != (-1)))
            {
                // If there were no matches throughout search.
                DistB = -1;
                // Initializing length of the first row (minus first cell).
                int RowLength = 2;

                lock (locker)
                {
                    // Creating right limit for the triangular search area.
                    int RightLimit = (NeurInInputIndex + MainWindow._image.Width - 1) + RowLength;
                    // Choosing cell located to the bottom-left from the cell occupied by the neuron.
                    // Using search in each cell of the triangular shaped area.
                    for (int Cell = (NeurInInputIndex + MainWindow._image.Width - 1); Cell > 0; Cell++)
                    {
                        if (((Cell * 4) + 2) < (MainWindow.MainInput.Length - 1))
                        {
                            if ((Cell >= 0) && ((Cell * 4) >= 0))
                            {
                                // Quit search if first pixel occurence took place.
                                if ((MainWindow.MainInput[4 * Cell] > 0) || (MainWindow.MainInput[(4 * Cell) + 1] > 0) || (MainWindow.MainInput[(4 * Cell) + 2] > 0))
                                {
                                    DistB = Cell;
                                    return;
                                }

                                // Move cell to the lower-left from it's row if it's location reached right limit.                   
                                if (Cell == RightLimit)
                                {
                                    Cell = (Cell - RowLength) + MainWindow._image.Width - 1;
                                    RightLimit = RightLimit + MainWindow._image.Width + 1;
                                    RowLength += 2;
                                }
                            }
                        }
                    }
                }
            }
        } //-->BottomSearch()

        // Searches for the two highest rgb values in the MainInput vector and chooses one with the lowest distance between the node and the value.
        public void CompareDistances()
        {
            if ((DistR == (-1)) && (DistL == (-1)) && (DistT == (-1)) && (DistB == (-1)))
            {
                // If there were no matches;
                return;
            }
            else
            {
                // First two values with highest rgb rate.
                int FirstRgb = DistR;
                int SecondRgb = DistR;

                // Searching for the first two highest rgb values among distance search results.
                int[] DistArr = { DistR, DistL, DistT, DistB };

                foreach (int i in DistArr)
                {
                    if (i != (-1))
                    {
                        FirstRgb = i;
                        SecondRgb = i;
                        break;
                    }
                }

                foreach (int i in DistArr)
                {
                    if (i != (-1))
                    {
                        // If rgb values are higher change the variable with max values.
                        if (((MainWindow.MainInput[4 * i] + MainWindow.MainInput[4 * i + 1] + MainWindow.MainInput[4 * i + 2]) / 3)
                        > ((MainWindow.MainInput[4 * FirstRgb] + MainWindow.MainInput[4 * FirstRgb + 1] + MainWindow.MainInput[4 * FirstRgb + 2]) / 3))
                        {
                            FirstRgb = i;
                        }
                        if (i != FirstRgb)
                        {
                            if (FirstRgb == SecondRgb)
                            {
                                SecondRgb = i;
                            }
                            else if (((MainWindow.MainInput[4 * i] + MainWindow.MainInput[4 * i + 1] + MainWindow.MainInput[4 * i + 2]) / 3)
                            > ((MainWindow.MainInput[4 * SecondRgb] + MainWindow.MainInput[4 * SecondRgb + 1] + MainWindow.MainInput[4 * SecondRgb + 2]) / 3))
                            {
                                SecondRgb = i;
                            }
                        }
                    }
                }

                // Distances' variables needed for further comparison
                int FirstDist;
                int SecondDist;
  
                // Calculating distances for both values depending on the location.
                if (FirstRgb == DistR)
                {
                    FirstDist = DistR - NeurInInputIndex;
                }
                else if (FirstRgb == DistL)
                {
                    FirstDist = NeurInInputIndex - DistL;
                }
                else if (FirstRgb == DistT)
                {
                    FirstDist = (DistT - NeurInInputIndex) / MainWindow._image.Width;
                }
                else
                {
                    FirstDist = (NeurInInputIndex - DistB) / MainWindow._image.Width;
                }

                if (SecondRgb == DistR)
                {
                    SecondDist = DistR - NeurInInputIndex;
                }
                else if (SecondRgb == DistL)
                {
                    SecondDist = NeurInInputIndex - DistL;
                }
                else if (SecondRgb == DistT)
                {
                    SecondDist = (DistT - NeurInInputIndex) / MainWindow._image.Width;
                }
                else
                {
                    SecondDist = (NeurInInputIndex - DistB) / MainWindow._image.Width;
                }

                // Variable used for further location determination (right, left, top or bottom). 
                bool IsFirst = false;

                // Choosing the nearest distance between the two
                if (FirstDist <= SecondDist)
                {
                    // calculating new error value depending on the position of the node in relation to the MainInput vector
                    Error += (int)Math.Pow(Math.Abs((FirstDist) * MainWindow._pixelSize), 2);
                    // Changing Delta (distance) in order to move winner and it's synapses as well as it's neighbours and their axons
                    Delta = NeuralNetwork.Eps_w * ((FirstDist));
                    
                    MainWindow.near1 = MainWindow.MainInput[4 * FirstRgb];
                    MainWindow.near2 = MainWindow.MainInput[4 * FirstRgb + 1];
                    MainWindow.near3 = MainWindow.MainInput[4 * FirstRgb + 2];
                    IsFirst = true;
                }
                else
                {
                    Error += (int)Math.Pow(Math.Abs((SecondDist) * MainWindow._pixelSize), 2);
                    // Changing Delta (distance) in order to move winner and it's synapses as well as it's neighbours and their axons
                    Delta = NeuralNetwork.Eps_w * ((SecondDist));

                    MainWindow.near1 = MainWindow.MainInput[4 * SecondRgb];
                    MainWindow.near2 = MainWindow.MainInput[4 * SecondRgb + 1];
                    MainWindow.near3 = MainWindow.MainInput[4 * SecondRgb + 2];
                }
                
                // Determing the direction where the node should be moved.
                if (IsFirst == true) 
                {
                    if (FirstRgb == DistR)
                    {
                        LeftNode = false;
                        TopNode = false;
                        BottomNode = false;
                        MinDist = DistR;
                    }
                    else if (FirstRgb == DistL)
                    {
                        LeftNode = true;
                        TopNode = false;
                        BottomNode = false;
                        MinDist = DistL;
                    }
                    else if (FirstRgb == DistT)
                    {
                        LeftNode = false;
                        TopNode = true;
                        BottomNode = false;
                        MinDist = DistT;
                    }
                    else
                    {
                        LeftNode = false;
                        TopNode = false;
                        BottomNode = true;
                        MinDist = DistB;
                    }
                }
                else
                {
                    if (SecondRgb == DistR)
                    {
                        LeftNode = false;
                        TopNode = false;
                        BottomNode = false;
                        MinDist = DistR;
                    }
                    else if (SecondRgb == DistL)
                    {
                        LeftNode = true;
                        TopNode = false;
                        BottomNode = false;
                        MinDist = DistL;
                    }
                    else if (SecondRgb == DistT)
                    {
                        LeftNode = false;
                        TopNode = true;
                        BottomNode = false;
                        MinDist = DistT;
                    }
                    else
                    {
                        LeftNode = false;
                        TopNode = false;
                        BottomNode = true;
                        MinDist = DistB;
                    }
                }
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
