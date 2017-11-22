using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
    public class InstallTree
    {
        public InstallTree()
        {
            Nodes = new Dictionary<string, InstallNode>();
        }
        public Dictionary<String, InstallNode> Nodes;
    }

    public class InstallNode
    {
        public string Name { get; set; }
        public bool Installed { get; set; }     

        public InstallNode(string Name)
        {
            this.Name = Name;
            Dependents = new List<InstallNode>();
            DependsOn = new List<InstallNode>();
            Installed = false;
        }
        public List<InstallNode> Dependents { get; }

        public List<InstallNode> DependsOn { get; }
    }

    public struct InstallResponse
    {
        public InstallTree Tree { get; set; }
        public bool Success { get; set; }
    }

    class Program
    {
        public static InstallTree installTree = new InstallTree();

        // Install the node into the tree
        public delegate void CommandFunctions(InstallTree tree, string [] args);

        public static Dictionary<String, CommandFunctions> functions = new Dictionary<string, CommandFunctions>();        


        static void Main(string[] args)
        {
            functions.Add("help", Help);
            functions.Add("print", PrintNodes);
            functions.Add("depend", Depend);
            functions.Add("clear", Clear);
            functions.Add("install", Install);
            functions.Add("uninstall", UnInstall);

            while(true)
            {
                Console.WriteLine("Enter command");
                var command = Console.ReadLine().ToLower().Trim();
                if (String.IsNullOrEmpty(command))
                {
                    Console.WriteLine("quiting");
                    break;
                }
                string [] commandSplit = command.Split(' ');
                if (!functions.ContainsKey(commandSplit[0]))
                {
                    Console.WriteLine("Command not recognized");
                    Program.Help(installTree, null);
                }
                else
                {
                    functions[commandSplit[0]](installTree, commandSplit.Skip(1).ToArray());
                }
            }
        }

        public static void PrintNodes (InstallTree tree, string [] args)
        {
            Console.WriteLine("Printing all Nodes");
            foreach(var node in tree.Nodes)
            {
                Console.WriteLine(node.Value.Name);                
            }
        }

        public static void Help(InstallTree tree, string [] args)
        {
            Console.Write("Commands are ");
            foreach (var function in functions)
            {
                Console.Write(function.Key + " ");
            }
        }

        public static void Depend(InstallTree tree, string [] args)
        {
            Console.WriteLine("Adding " + args[0]);
            Console.WriteLine("Depending on " + String.Join(", ", args.Skip(1).ToArray()));
            var node = new InstallNode(args[0]);
            var response = InstallMethods.AddNode(tree, args[0], args.Skip(1).ToList());
            if (response.Success) { Console.WriteLine("Dependency addition Successful"); }
            else { Console.WriteLine("Dependency addition Failed"); }
        }

        public static void Clear(InstallTree tree, string [] arg)
        {
            Console.WriteLine("Clearing Install Tree");
            tree = new InstallTree();
        }

        public static void Install(InstallTree tree, string[] arg)
        {
            Console.WriteLine("Installing " + arg[0]);
            var result = InstallMethods.InstallNodeInTree(tree, arg[0]);
            if (result.Success) { Console.WriteLine("Install Successful"); }
            else { Console.WriteLine("Install Unsuccessful"); }
        }

        public static void UnInstall(InstallTree tree, string[] arg)
        {
            Console.WriteLine("UnInstalling " + arg[0]);
            var result = InstallMethods.UnInstallNodeInTree(tree, arg[0]);
            if (result.Success) { Console.WriteLine("UnInstall Successful"); }
            else { Console.WriteLine("UnInstall Unsuccessful"); }
        }

    }
}
