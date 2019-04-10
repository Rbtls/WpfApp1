using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class NeuralNetwork
    {
        //Main number of neurons
        private long NeursCount;
        public float Error_min { get; set; }
        public Neuron Winner;
        public Neuron SecondWinner;
        public int Iteration_number { get; set; }
        public float MaxLocalError { get; set; }
        public int Max_nodes { get; set; }
        public int Lambda { get; set; }
        public int Age_max { get; set; }
        //alpha & beta are used to adapt errors
        public float Alpha { get; set; }
        public float Beta { get; set; }                         //////////////////////
        //eps_w & eps_n are used to adapt weights
        public static float Eps_w { get; set; }
        public static float Eps_n { get; set; }

        public LinkedList<Connection> ConnectionsList = new LinkedList<Connection>();

        public NeuralNetwork()
        {
            // Network parameters
            Lambda = 20;  // frequency used for new node addition
            Age_max = 15; // maximum connection age
            Alpha = 0.5f; // variable used for mistakes adaptation
            Eps_w = 0.028f; // Eps_w and Eps_n are used for weights adaptation (the distance between the node and the main input vector)
            Eps_n = 0.0006f;
            Max_nodes = 100; // maximum amount of nodes

            // Creating new connection
            NeursCount = 0;
            Connection _conn = new Connection(ref NeursCount);
            ConnectionsList.AddFirst(_conn);

            // Debug
            //////////////////////adding new neuron
            // adding new connection info (input) to the existing parent neuron (first parameter)
            //Connection _conn2 = new Connection(ref ConnectionsList.Last.Value.SecondNeurInConn, _conn.ConnId, ref NeursCount);

            // adding new connection to the ConnectionsList
           // ConnectionsList.AddLast(_conn2);

        }

        // set index in input vector (the space occupied by neuron)  
        public void SetInd(int ConnInd, int ind)
        {
            ConnectionsList.ElementAt(ConnInd).FirstNeurInConn.NeurInInputIndex = ind;
        }

        // get number of neurons in the network
        public long GetNetworkNeurCount()
        {
            return NeursCount;
        }

        private Neuron FindNeuron(long uid)
        {
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.FirstNeurInConn.NeurId == uid)
                {
                    return _conn.FirstNeurInConn;
                }
                else if (_conn.SecondNeurInConn.NeurId == uid)
                {
                    return _conn.SecondNeurInConn;
                }
            }
            return null;
        } //-->FindNeuron

        private void FindWinners() 
        {
            if (Error_min == 0)
            {
                Error_min = ConnectionsList.ElementAt(0).FirstNeurInConn.Error;
                Winner = ConnectionsList.ElementAt(0).FirstNeurInConn;
                SecondWinner = ConnectionsList.ElementAt(0).SecondNeurInConn; ;
            }
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.FirstNeurInConn.Error < Error_min)
                {
                    SecondWinner = Winner;
                    Error_min = _conn.FirstNeurInConn.Error;
                    Winner = _conn.FirstNeurInConn;
                }
                else if (_conn.SecondNeurInConn != null)
                {
                    if (_conn.SecondNeurInConn.Error < Error_min)
                    {
                        SecondWinner = Winner;
                        Error_min = _conn.SecondNeurInConn.Error;
                        Winner = _conn.SecondNeurInConn;
                    }
                }
            }
        } //-->FindWinners

        public void ProcessVector()
        {
           // while ((MainWindow.Stop == false) || (NeursCount < Max_nodes))
           // {
                //0. Increase iteration number
                Iteration_number++;

                //1.
                FindWinners();

                //2. Searching for the nearest value in the _Input vector (this way we can calculate the distance between the node and the value)

                Parallel.Invoke(
                    () => Winner.ForwardSearch(),
                    () => Winner.BackwardSearch(),
                    () => Winner.TopSearch(),
                    () => Winner.BottomSearch()
                );
                // Finding shortest distance that will be the weight of the winner node.
                Winner.CompareDistances();

                //3. Changing winner's local error
                Winner.E += Winner.Error;

                //4. Move the winner and all of it's topological neighbours towards _Input vector using Delta value (calculated with Eps_w)
                Adapt_weights(Winner.NeurId, true);
                MoveNeighbours(Winner.NeurId);

                //5. Increase age of all connections coming out from the winner (axons) by 1 
                for (int i = 0; i < Winner.AxonsWeights.Count; i++)
                {
                    foreach (Connection _conn in ConnectionsList)
                    {
                        if (_conn.ConnId == Winner.AxonsWeights[i].ConnId)
                        {
                            _conn.ConnAge += 1;
                        }
                    }
                }

                //6. If there is a connection between the first winner and the second winner change it's age to 0, else - create a connection between them
                bool IsConnected = false;
                for (int i = 0; i < Winner.AxonsWeights.Count; i++)
                {
                    if (Winner.AxonsWeights[i].NeighId == SecondWinner.NeurId)
                    {
                        foreach (Connection _conn in ConnectionsList)
                        {
                            if (_conn.ConnId == Winner.AxonsWeights[i].ConnId)
                            {
                                _conn.ConnAge = 0;
                            }
                        }
                        IsConnected = true;
                    }
                }
                if (IsConnected == false)
                {
                    for (int i = 0; i < SecondWinner.AxonsWeights.Count; i++)
                    {
                        if (SecondWinner.AxonsWeights[i].NeighId == Winner.NeurId)
                        {
                            foreach (Connection _conn in ConnectionsList)
                            {
                                if (_conn.ConnId == SecondWinner.AxonsWeights[i].ConnId)
                                {
                                    _conn.ConnAge = 0;
                                }
                            }
                            IsConnected = true;
                        }
                    }
                    // If there's no connection between the first winner and the second winner (or vice versa) create new connection between them 
                    if (IsConnected == false)
                    {
                        Connection winners_conn = new Connection(ref Winner, ref SecondWinner, ConnectionsList.Last.Value.ConnId);
                        ConnectionsList.AddLast(winners_conn);
                        IsConnected = true;
                    }
                }

                //7. Remove all connections with age greater than Age_max.
                // Trigger sygnaling the end of foreach search to stop reiteration of aforementioned search.
                int Trig = 0;
                // Variable representing each iteration of foreach search.
                int LoopCount = 0;
                while (true)
                {
                    LoopCount = 0;
                    foreach (Connection _conn in ConnectionsList)
                    {
                        if (_conn.ConnAge > Age_max)
                        {
                            // remove any info about inputs/outputs between two neurons in the connection
                            for (int i = 0; i < _conn.FirstNeurInConn.AxonsWeights.Count; i++)
                            {
                                if (_conn.FirstNeurInConn.AxonsWeights.ElementAt(i).NeighId == _conn.SecondNeurInConn.NeurId)
                                {
                                    _conn.FirstNeurInConn.AxonsWeights.RemoveAt(i);
                                }
                            }
                            for (int i = 0; i < _conn.SecondNeurInConn.SynapsesWeights.Count; i++)
                            {
                                if (_conn.SecondNeurInConn.SynapsesWeights.ElementAt(i).NeighId == _conn.FirstNeurInConn.NeurId)
                                {
                                    _conn.SecondNeurInConn.SynapsesWeights.RemoveAt(i);
                                }
                            }
                            ConnectionsList.Remove(_conn);
                            break;
                        }
                        // The end of foreach search (no need to reiterate)
                        if (LoopCount == (ConnectionsList.Count - 1))
                        {
                            Trig = 1;
                        }
                        LoopCount += 1;
                    }
                    if (Trig == 1)
                    {
                        break;
                    }
                }

                Trig = 0;
                LoopCount = 0;
                while (true)
                {
                    LoopCount = 0;
                    foreach (Connection _conn in ConnectionsList)
                    {
                        // If after deletion there are any neurons without connections left - remove them.
                        if ((_conn.FirstNeurInConn.AxonsWeights.Count == 0) && (_conn.FirstNeurInConn.SynapsesWeights.Count == 0))
                        {
                            _conn.FirstNeurInConn = null;
                        }
                        if ((_conn.SecondNeurInConn.AxonsWeights.Count == 0) && (_conn.SecondNeurInConn.SynapsesWeights.Count == 0))
                        {
                            _conn.SecondNeurInConn = null;
                        }
                        // If there's a connection without neurons - remove it.
                        if ((_conn.FirstNeurInConn == null) && (_conn.SecondNeurInConn == null))
                        {
                            ConnectionsList.Remove(_conn);
                            break;
                        }
                        // the end of foreach search (no need to reiterate)
                        if (LoopCount == (ConnectionsList.Count - 1))
                        {
                            Trig = 1;
                        }
                        LoopCount += 1;
                    }
                    if (Trig == 1)
                    {
                        break;
                    }
                }

                //8. If the iteration number equals Lambda and there's no limit reached - create new node with the rules described below.
                if ((NeursCount > 2) && (Iteration_number % Lambda == 0) && (NeursCount < Max_nodes))
                {
                    // Find neuron with the largest local error.
                    Neuron NeurMaxLocalE = FindMaxLocalError();

                    // Among the neighbours with the largest local error find the one with the largest local error.
                    Neuron NeighMaxLocalE = FindNeighMaxLocalError(ref NeurMaxLocalE);

                    // Search for the weight of the node (DistL & DistR values).
                    Parallel.Invoke(
                    () => NeurMaxLocalE.ForwardSearch(),
                    () => NeurMaxLocalE.BackwardSearch(),
                    () => NeurMaxLocalE.TopSearch(),
                    () => NeurMaxLocalE.BottomSearch()
                    );
                    // Find the nearest value in Input vector in relation to the node.
                    NeurMaxLocalE.CompareDistances();

                    // Search for the weight of the neighbour node.
                    Parallel.Invoke(
                    () => NeighMaxLocalE.ForwardSearch(),
                    () => NeighMaxLocalE.BackwardSearch(),
                    () => NeighMaxLocalE.TopSearch(),
                    () => NeighMaxLocalE.BottomSearch()
                    );
                    // Find the nearest value in Input vector in relation to the node.
                    NeighMaxLocalE.CompareDistances();
                                       
                    // Create new neuron between two previous neurons and replace connections accordingly.
                    // Reduce error values of the previous neurons and replace error value of the newly placed neuron.
                    InsertNeuron(ref NeurMaxLocalE, ref NeighMaxLocalE);        
                }
                else if (NeursCount == 2)
                {
                    InsertNeuron(ref Winner, ref SecondWinner);
                }

                //9. Decrease all error variables for all nodes by a Beta value
                foreach (Connection _conn in ConnectionsList)
                {
                    _conn.FirstNeurInConn.E *= Beta;
                    _conn.SecondNeurInConn.E *= Beta;
                }
            //}
        } //-->ProcessVector

        // Change neuron's and it's neighbours' positions 
        public void Adapt_weights(long parent_id, bool isWinner)
        {
            Neuron ParentNeur;

            // Reference to the parent neuron in the connection
            if (FindNeuron(parent_id) != null)
            {
                ParentNeur = FindNeuron(parent_id);
            }
            else
            {
                return;
            }

            // Check whether the node is the winner node (Eps_w is for the winner, Eps_n - for the neighbours)
            if (isWinner == false)
            {
                ParentNeur.Delta /= Eps_w;
                ParentNeur.Delta *= Eps_n;
            }

            if (ParentNeur.Left == false )
            {
                // If input array is to the right from the neuron's position increase coordinates' values by the amount of Delta value
                if ((ParentNeur.Top == false) && (ParentNeur.Bottom == false))
                {
                    // Calculate the increase value (check whether delta + x is out of Vpw borders)
                    float IncRowsL = (ParentNeur.Delta + ParentNeur.Neur_X) / (MainWindow.Vpw - (2 * MainWindow._frame)); //should be vpw - borders!

                    // When Delta is out of the X limit, increase Y
                    if (IncRowsL > 1)
                    {
                        // Increasing Y by the amount of rows            
                        ParentNeur.Neur_Y += (int)IncRowsL / MainWindow._pixelSize;

                        // Reducing Delta value by the amount of rows for further increase of the X value
                        ParentNeur.Neur_X += (ParentNeur.Delta - (int)IncRowsL);

                        // Assigning new Index value due to the change in coordinates
                        ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                        // Changing coordinates for visualisation
                        ParentNeur.ChangePosition();
                    }
                    else
                    {
                        // Increasing X value by the amount of Delta value
                        ParentNeur.Neur_X += ParentNeur.Delta;

                        // Assigning new Index value due to the change in coordinates
                        ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                        // Changing coordinates for visualisation
                        ParentNeur.ChangePosition();
                    }
                }
            }
            // If input array is to the left from the neuron's position decrease coordinates' values by the amount of Delta value
            else if (ParentNeur.Left == true) 
            {
                // Calculate the increase value
                float IncRowsR = (ParentNeur.Neur_X - ParentNeur.Delta) / (MainWindow.Vpw - 2 * MainWindow._frame); //should be vpw - borders!

                if (IncRowsR < 0)
                {
                    // Decreasing Y by removing the integer part of Delta/X division (the amount of rows with width==X each)
                    ParentNeur.Neur_Y -= (int)(ParentNeur.Delta / ParentNeur.Neur_X) / MainWindow._pixelSize;

                    // Reducing Delta value by the amount of rows for further decrease of the X value
                    ParentNeur.Neur_X -= (ParentNeur.Delta - (ParentNeur.Neur_Y * ParentNeur.Neur_X));

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // Decreasing X value by the amount of Delta value
                    ParentNeur.Neur_X -= ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }

            if (ParentNeur.Top == true)
            {
                if ((ParentNeur.Neur_Y + ParentNeur.Delta) < MainWindow._image.Height)
                {
                    // Increasing Y value by the amount of Delta value
                    ParentNeur.Neur_Y += ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // If Y is above picture's borders
                    ParentNeur.Neur_Y = MainWindow._image.Height - MainWindow.NodeScale;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
            else if (ParentNeur.Bottom == true)
            {
                if ((ParentNeur.Neur_Y - ParentNeur.Delta) > 0)
                {
                    // Increasing Y value by the amount of Delta value
                    ParentNeur.Neur_Y -= ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // If Y is below picture's borders
                    ParentNeur.Neur_Y = 0;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurInInputIndex = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
            MainWindow.GlRender();
        } //-->Adapt_weights

        public void MoveNeighbours(long parent_id)   
        {
            Neuron ParentNeur;

            // reference to the parent neuron in the connection
            if (FindNeuron(parent_id) != null)
            {
                ParentNeur = FindNeuron(parent_id);
            }
            else
            {
                return;
            }

            // Find neighbours' ids for further search of any connections related to the parent neuron
            for (int i = 0; i < ParentNeur.AxonsWeights.Count; i++)
            {
                // Find neighbours' axons values related to the parent neuron for neighbours' position change towards MainInput
                Adapt_weights(ParentNeur.AxonsWeights[i].NeighId, false);
            }

            for (int i = 0; i < ParentNeur.SynapsesWeights.Count; i++)
            {
                Adapt_weights(ParentNeur.SynapsesWeights[i].NeighId, false);
            }
        } //-->MoveNeighbours

        public ref Neuron FindMaxLocalError()
        {
            // Find neuron with the largest local error.
            MaxLocalError = 0;
            ref Neuron NeurMax = ref Winner;
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.FirstNeurInConn.E > MaxLocalError)
                {
                    MaxLocalError = _conn.FirstNeurInConn.E;
                    NeurMax = _conn.FirstNeurInConn;
                }
                else if (_conn.SecondNeurInConn.E > MaxLocalError)
                {
                    MaxLocalError = _conn.SecondNeurInConn.E;
                    NeurMax = _conn.SecondNeurInConn;
                }
            }
            return ref NeurMax;
        } //-->FindMaxLocalError

        public ref Neuron FindNeighMaxLocalError(ref Neuron NeurMaxLocalE)
        {
            // Among the neighbours with the largest local error find the one with the largest local error.
            float MaxLocalE = 0;
            ref Neuron NeighMaxLocalE = ref SecondWinner;
            foreach (ConnWeights _synapse in NeurMaxLocalE.SynapsesWeights)
            {
                Neuron Neighbour = FindNeuron(_synapse.NeighId);
                if (Neighbour.E > MaxLocalE)
                {
                    MaxLocalE = Neighbour.E;
                    NeighMaxLocalE = Neighbour;
                }
            }
            foreach (ConnWeights _axon in NeurMaxLocalE.AxonsWeights)
            {
                Neuron Neighbour = FindNeuron(_axon.NeighId);
                if (Neighbour.E > MaxLocalE)
                {
                    MaxLocalE = Neighbour.E;
                    NeighMaxLocalE = Neighbour;
                }
            }
            return ref NeighMaxLocalE;
        } //-->FindNeighMaxLocalError

        // Inserts new neuron between already established connection
        public void InsertNeuron(ref Neuron parent_node, ref Neuron child_node)
        {
            float NodeR_X = 0f;
            float NodeR_Y = 0f;

            if (parent_node.Neur_X >= child_node.Neur_X)
            {
                NodeR_X = child_node.Neur_X + ((parent_node.Neur_X - child_node.Neur_X) / 2f);
            }
            else
            {
                NodeR_X = parent_node.Neur_X + ((child_node.Neur_X - parent_node.Neur_X) / 2f);
            }

            if (parent_node.Neur_Y >= child_node.Neur_Y)
            {
                NodeR_Y = child_node.Neur_Y + ((parent_node.Neur_Y - child_node.Neur_Y) / 2f);
            }
            else
            {
                NodeR_Y = parent_node.Neur_Y + ((child_node.Neur_Y - parent_node.Neur_Y) / 2f);
            }

            if (NodeR_X > 1.0f)
            {
                NodeR_X = 1.0f;
            }
            if (NodeR_Y > 1.0f)
            {
                NodeR_Y = 1.0f;
            }

            // Creating new node R. Node's position has to be inbetween two previous nodes.
            Neuron Node_R = new Neuron(++NeursCount, NodeR_X, NodeR_Y);

            // Adding two connections (U and V) between R and two previous nodes
            Connection Conn_U = new Connection(ref parent_node, ref Node_R, ConnectionsList.Last.Value.ConnId);
            ConnectionsList.AddLast(Conn_U);

            Connection Conn_V = new Connection(ref child_node, ref Node_R, ConnectionsList.Last.Value.ConnId);
            ConnectionsList.AddLast(Conn_V);

            // Weight of R (the distance between the node and the main input vector) equals sum of weights of two previous nodes divided by two.
            // Choosing weight value of each node. 
            if ((parent_node.Left == false) && (child_node.Left == false))
            {
                // If two previous nodes were located to the right from input vector.
                if ((parent_node.Top == false) && (child_node.Top == false) && (parent_node.Bottom == false) && (child_node.Bottom == false))
                {
                    Node_R.DistR = (parent_node.DistR + child_node.DistR) / 2;
                    Node_R.Left = false;
                    Node_R.Top = false;
                    Node_R.Bottom = false;
                }
                else if ((parent_node.Top == true) && (child_node.Top == true))
                {
                    Node_R.DistT = (parent_node.DistT + child_node.DistT) / 2;
                    Node_R.Left = false;
                    Node_R.Top = true;
                    Node_R.Bottom = false;               
                }
                else if ((parent_node.Top == true) && (child_node.Bottom == true))
                {
                    if (parent_node.DistT > child_node.DistB)
                    {
                        Node_R.DistB = (parent_node.DistT + child_node.DistB) / 2;
                        Node_R.Left = false;
                        Node_R.Top = false;
                        Node_R.Bottom = true;
                    }
                    else
                    {
                        Node_R.DistT = (parent_node.DistT + child_node.DistB) / 2;
                        Node_R.Left = false;
                        Node_R.Top = true;
                        Node_R.Bottom = false;
                    }
                }
                else if ((parent_node.Bottom == true) && (child_node.Top == true))
                {
                    if (parent_node.DistB > child_node.DistT)
                    {
                        Node_R.DistT = (parent_node.DistB + child_node.DistT) / 2;
                        Node_R.Left = false;
                        Node_R.Top = true;
                        Node_R.Bottom = false;
                    }
                    else
                    {
                        Node_R.DistB = (parent_node.DistB + child_node.DistT) / 2;
                        Node_R.Left = false;
                        Node_R.Top = false;
                        Node_R.Bottom = true;
                    }
                }
                else if ((parent_node.Bottom == true) && (child_node.Bottom == true))
                {
                    Node_R.DistB = (parent_node.DistB + child_node.DistB) / 2;
                    Node_R.Left = false;
                    Node_R.Top = false;
                    Node_R.Bottom = true;
                }

            }
            else if (((parent_node.Left == true) && (child_node.Left == false)) || ((parent_node.Left == false) && (child_node.Left == true)))
            {
                if (parent_node.DistL > child_node.DistR)
                {
                    Node_R.DistR = (parent_node.DistR + child_node.DistL) / 2;
                    Node_R.Left = false;
                    Node_R.Top = false;
                    Node_R.Bottom = false;
                }
                else
                {
                    Node_R.DistL = (parent_node.DistR + child_node.DistL) / 2;
                    Node_R.Left = true;
                    Node_R.Top = false;
                    Node_R.Bottom = false;
                }
            }
            else if ((parent_node.Left == true) && (child_node.Left == true))
            {
                Node_R.DistL = (parent_node.DistL + child_node.DistL) / 2;
                Node_R.Left = true;
                Node_R.Top = false;
                Node_R.Bottom = false;
            }

            // Removing connection between two previous nodes (if such connection exists)
            for (int i = 0; i < parent_node.AxonsWeights.Count; i++)
            {
                if (parent_node.AxonsWeights.ElementAt(i).NeighId == child_node.NeurId)
                {
                    parent_node.AxonsWeights.RemoveAt(i);
                }
            }
            for (int i = 0; i < child_node.SynapsesWeights.Count; i++)
            {
                if (child_node.SynapsesWeights.ElementAt(i).NeighId == parent_node.NeurId)
                {
                    child_node.SynapsesWeights.RemoveAt(i);
                }
            }
            foreach (Connection _conn in ConnectionsList)
            {
                if (((_conn.FirstNeurInConn == parent_node) && (_conn.SecondNeurInConn == child_node)) || ((_conn.FirstNeurInConn == child_node) && (_conn.SecondNeurInConn == parent_node)))
                {
                    ConnectionsList.Remove(_conn);
                    break;
                }
            }
            
            // Decreasing error values for both previous nodes and setting new error value for R
            parent_node.E = Alpha * parent_node.E;
            child_node.E = Alpha * child_node.E;
            Node_R.E = parent_node.E;
            MainWindow.GlRender();
        } //-->InsertNeuron

    } //-->NeuralNetwork
}
