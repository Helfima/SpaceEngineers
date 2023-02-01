using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinematics
{
    class Program
    {
        static void Main(string[] args)
        {
            // Longueurs des bras
            double l1, l2, l3, l4;

            // Saisir les longueurs des bras
            Console.Write("Saisir l1 : ");
            l1 = double.Parse(Console.ReadLine());
            Console.Write("Saisir l2 : ");
            l2 = double.Parse(Console.ReadLine());
            Console.Write("Saisir l3 : ");
            l3 = double.Parse(Console.ReadLine());
            Console.Write("Saisir l4 : ");
            l4 = double.Parse(Console.ReadLine());

            // Point final
            double x, y, z;

            // Saisir les coordonnées du point final
            Console.Write("Saisir x : ");
            x = double.Parse(Console.ReadLine());
            Console.Write("Saisir y : ");
            y = double.Parse(Console.ReadLine());
            Console.Write("Saisir z : ");
            z = double.Parse(Console.ReadLine());

            // Calculer les angles
            double theta1 = Math.Atan2(y, x);
            double r = Math.Sqrt(x * x + y * y) - l1;
            double c = z - l2;
            double alpha = Math.Atan2(c, r);
            double beta = Math.Acos((l3 * l3 + r * r + c * c - l4 * l4) / (2 * l3 * Math.Sqrt(r * r + c * c)));
            double theta3 = Math.PI - beta;
            double gamma = Math.Acos((l3 * l3 + l4 * l4 - r * r - c * c) / (2 * l3 * l4));
            double theta2 = alpha - gamma;

            // Afficher les angles
            Console.WriteLine("Theta 1 : " + theta1);
            Console.WriteLine("Theta 2 : " + theta2);
            Console.WriteLine("Theta 3 : " + theta3);

            Console.ReadKey();
        }
    }
}

