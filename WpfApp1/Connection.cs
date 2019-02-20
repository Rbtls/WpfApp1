using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Connection
    {
        // connection id
        public int ConnId { get; set; }
        // first & second neurons in the connection
        public Neuron ConnNeur1;
        public Neuron ConnNeur2;
        // ids of nodes that should be connected 
        public long Node_id1 { get; set; }
        public long Node_id2 { get; set; }
        // connection weight & age
        public float ConnWeight { get; set; }
        public int ConnAge { get; set; }
        // random value for new node's location
        private static Random rnd = new Random();

        // connection between the first two neurons when we don't have the id of the parent neuron
        public Connection(ref long num)
        {
            ConnId = 0;
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            ConnNeur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Node_id1 = ConnNeur1.NeurId;

            var TempX_2 = MainWindow.Resln * (rnd.Next(0, 9));
            var TempY_2 = MainWindow.Resln * (rnd.Next(0, 9));
            ConnNeur2 = new Neuron(++num, TempX_2, TempY_2);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Node_id2 = ConnNeur2.NeurId;

            //adding output info to neur1
            ConnNeur1.AxonsWeights.Add(new ConnWeights(ConnId, 1, ConnNeur2.NeurId, TempX_2, TempY_2));

            //adding input info to neur2
            ConnNeur2.SynapsesWeights.Add(new ConnWeights(ConnId, 1, ConnNeur1.NeurId, TempX_1, TempY_1));

            MainWindow.GlRender();
        }

        // creating connection when parent neuron already exists
        public Connection(ref Neuron parent_node, int previous_conn_id, ref long num)
        {
            long parent_node_id = parent_node.NeurId;
            float X_parent = parent_node.Neur_X;
            float Y_parent = parent_node.Neur_Y;
            
            ConnId = ++previous_conn_id;
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            ConnNeur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Node_id1 = ConnNeur1.NeurId;

            //adding output info to _neur1.Axons array, Axons.Count being new id for the new weight that's being added
            ConnNeur1.AxonsWeights.Add(new ConnWeights(ConnId, ConnNeur1.AxonsWeights.Count + 1, parent_node_id, X_parent, Y_parent));

            // adding input info to parent node in the first connection 
            // "1" is the first weight_id for this neuron (_conn2.ConnNeur2)
            parent_node.SynapsesWeights.Add(new ConnWeights(ConnId, 1, ConnNeur1.NeurId, ConnNeur1.Neur_X,
                ConnNeur1.Neur_Y));

            //send info about which neuron should be connected to _neur1 to the list of connections in the opengl mode
            MainWindow._ConnEdges.Add((ushort)(parent_node_id + (2 * parent_node_id)));
            Node_id2 = parent_node_id;

            MainWindow.GlRender();
        }
    } //-->Connection
}
