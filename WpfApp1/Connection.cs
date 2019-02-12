using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Connection
    {
        public Neuron ConnNeur1 { get; set; }
        public Neuron ConnNeur2 { get; set; }
        public float ConnWeight { get; set; }
        public int ConnAge { get; set; }
        //ids of nodes that should be connected 
        public long Conn_id1 { get; set; } 
        public long Conn_id2 { get; set; }
        private static Random rnd = new Random();

        //connection between first two neurons when we don't have the id of the parent neuron
        public Connection(ref long num)
        {
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            ConnNeur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Conn_id1 = ConnNeur1.NeurNum;

            var TempX_2 = MainWindow.Resln * (rnd.Next(0, 9));
            var TempY_2 = MainWindow.Resln * (rnd.Next(0, 9));
            ConnNeur2 = new Neuron(++num, TempX_2, TempY_2);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Conn_id2 = ConnNeur2.NeurNum;

            //adding output info to neur1
            ConnNeur1.Axons.Add(new NumWeights(1, ConnNeur2.NeurNum, TempX_2, TempY_2));

            //adding input info to neur2
            ConnNeur2.Synapses.Add(new NumWeights(1, ConnNeur1.NeurNum, TempX_1, TempY_1));

            MainWindow.GlRender();
        }

        // Creating connection when parent neuron already exists
        public Connection(ref long num, long previous_id, float X_parent, float Y_parent)
        {
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            ConnNeur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Conn_id1 = ConnNeur1.NeurNum;

            //adding output info to _neur1.Axons array, Axons.Count being new id for the new weight that's being added
            ConnNeur1.Axons.Add(new NumWeights(ConnNeur1.Axons.Count, previous_id, X_parent, Y_parent));

            //send info (about which neuron should be connected to _neur1) to the list of connections in the opengl mode
            MainWindow._ConnEdges.Add((ushort)(previous_id + (2 * previous_id)));
            Conn_id2 = previous_id;

            MainWindow.GlRender();
        }
    } //-->Connection
}
