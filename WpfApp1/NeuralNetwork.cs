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
        public int Error_min { get; set; }
        public Neuron Winner;
        public Neuron SecondWinner;
        public int Iteration_number { get; set; }

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
            Connection _conn2 = new Connection(ref ConnectionsList.Last.Value.SecondNeurInConn, _conn.ConnId, ref NeursCount);

            // adding new connection to the ConnectionsList
            ConnectionsList.AddLast(_conn2);

        }

        // set index in input vector (the space occupied by neuron)  //////WIP/////////
        public void SetInd(int ConnInd, int ind)
        {
            ConnectionsList.ElementAt(ConnInd).FirstNeurInConn.NeurIndInput = ind;
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
        }

        private void FindWinners() ////////////////Work In Progress////////////
        {
            if (Error_min == 0)
            {
                Error_min = ConnectionsList.ElementAt(0).FirstNeurInConn.Error;
                Winner = ConnectionsList.ElementAt(0).FirstNeurInConn;
                SecondWinner = Winner;
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
        }

        public void ProcessVector()
        {
            //0. Increase iteration number
            Iteration_number++;

            //1.
            FindWinners();

            //2. Searching for the nearest value in the _Input vector (this way we can calculate the distance between the node and the value)

            Parallel.Invoke(
                () => Winner.ForwardSearch(),
                () => Winner.BackwardSearch()
            );

            Winner.CompareDistances();

            //3. Changing winner's local error
            Winner.E += Winner.Error;

            //4. Move the winner and all of it's topological neighbours towards _Input vector using Delta value (calculated with Eps_w)
            Adapt_weights(Winner.NeurId, true);
            MoveNeighbours(Winner.NeurId);

            //5. Increase age of all connections coming out from the winner (axons) by 1 
            for (int i = 0; i < Winner.AxonsWeights.Count; i++)
            {
                ConnectionsList.ElementAt(Winner.AxonsWeights[i].ConnId).ConnAge += 1;
            }

            //6. If there is a connection between the first winner and the second winner change it's age to 0, else - create a connection between them
            bool IsConnected = false;
            for (int i = 0; i < Winner.AxonsWeights.Count; i++)
            {
                if (Winner.AxonsWeights[i].NeighId == SecondWinner.NeurId)
                {
                    ConnectionsList.ElementAt(Winner.AxonsWeights[i].ConnId).ConnAge = 0;
                    IsConnected = true;
                }
            }
            if (IsConnected == false)
            {
                for (int i = 0; i < SecondWinner.AxonsWeights.Count; i++)
                {
                    if (SecondWinner.AxonsWeights[i].NeighId == Winner.NeurId)
                    {
                        ConnectionsList.ElementAt(SecondWinner.AxonsWeights[i].ConnId).ConnAge = 0;
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
                    if ( LoopCount == (ConnectionsList.Count - 1))
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

            //8. If the iteration number equals Lambda and there's no limit reached - create new node with rules described below.

        }

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

            // If neuron's position is to the left of the input array increase coordinates' values by the amount of Delta value
            if (ParentNeur.Left == false)
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
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // Increasing X value by the amount of Delta value
                    ParentNeur.Neur_X += ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
            else // If neuron's position is to the right of the input array decrease coordinates' values by the amount of Delta value
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
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // Decreasing X value by the amount of Delta value
                    ParentNeur.Neur_X -= ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
        }

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
        }

    } //-->NeuralNetwork
}
