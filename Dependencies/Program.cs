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


    public static class InstallMethods
    {
        public static InstallResponse AddNode(InstallTree Tree, string Name, List<String> DependsOn)
        {
            if (Tree == null)
            {
                return new InstallResponse() { Tree = new InstallTree(), Success = false };
            }
            var response = new InstallResponse();

            InstallNode addNode = null;
            if (!Tree.Nodes.TryGetValue(Name, out addNode))
            {
                Tree.Nodes[Name] = addNode = new InstallNode (Name);
            }
            
            response.Tree = Tree;            

            var depResp = AddDepends(Tree, addNode, DependsOn);

            return depResp;            
        }

        public static InstallResponse AddDepends(InstallTree tree, InstallNode addNode, List<String> DependsOn)
        {
            if (DependsOn == null) return new InstallResponse() { Success = true, Tree = tree };
            foreach (var depends in DependsOn)
            {
                InstallNode n = null;
                if (tree.Nodes.ContainsKey(depends))
                {
                    n = tree.Nodes[depends];
                }
                else
                {
                    n = new InstallNode(depends);
                    tree.Nodes[depends] = n;
                }
                
                addNode.DependsOn.Add(n);
                if (DetectDepends(n, addNode))
                {
                    return new InstallResponse() { Success = false, Tree = tree };
                };
                n.Dependents.Add(addNode);

            }
            tree.Nodes[addNode.Name] = addNode;

            return new InstallResponse() { Success = true, Tree = tree };
        }


        public static bool DetectDepends(InstallNode nodeA, InstallNode nodeB)
        {            
            if (nodeA.DependsOn.Count() == 0) return false;
            foreach (var dependent in nodeA.DependsOn)
            {
                if (dependent.Name == nodeB.Name) return true;
                if (DetectDepends(dependent, nodeB)) return true;
            }
            return false;
        }

        public static InstallResponse InstallNodeInTree(InstallTree tree, string Name)
        {
            if (!tree.Nodes.ContainsKey(Name))
            {
                return new InstallResponse()
                {
                    Tree = tree,
                    Success = false
                };
            };
            var node = tree.Nodes[Name];
            InstallNode(node);
            return new InstallResponse() { Tree = tree, Success = true };
        }

        private static void InstallNode(InstallNode node)
        {
            Console.WriteLine("Attempting to Install " + node.Name);
            if (node.Installed)
            {
                Console.WriteLine("Already Installed, skipping " + node.Name);
                return;
            }
            if (node.DependsOn.Count() == 0)
            {
                node.Installed = true;
                Console.WriteLine("Installing " + node.Name);
                return;
            }
            foreach (var depends in node.DependsOn)
            {
                InstallNode(depends);
            }
            node.Installed = true;
            Console.WriteLine("Installing " + node.Name);
            return;
        }

        public static InstallResponse UnInstallNodeInTree(InstallTree tree, string Name)
        {
            Console.WriteLine("Attempting to uninstall " + Name);
            if (!tree.Nodes.ContainsKey(Name))
            {
                return new InstallResponse()
                {
                    Tree = tree,
                    Success = false
                };
            };

            UnInstallNode(tree.Nodes[Name]);
            return new InstallResponse();
        }

        private static void UnInstallNode(InstallNode node)
        {
            if (!node.Installed)
            {
                Console.WriteLine("Already uninstalled, no action needed");
            }
            foreach (var d in node.Dependents)
            {
                if (d.Installed)
                {
                    Console.WriteLine("Node " + d.Name + " is still installed and depends on " + node.Name + " can't uninstall");
                    return;
                }
            }
            Console.WriteLine("No dependent nodes are installed, Uninstalling: " + node.Name);
            node.Installed = false;
            return;
        }

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
            var response = InstallMethods.AddDepends(tree, node, args.Skip(1).ToList());
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
