using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies.Tests
{
    [TestClass()]
    public class InstallMethodsTests
    {
        [TestMethod()]        
        public void AddNode_NullTreeError()
        {
            var response = InstallMethods.AddNode(null, "test", null);
            Assert.IsFalse(response.Success);
        }

        [TestMethod()]
        public void AddNode_Empty_Tree()
        {
            string name = "testNode";
            // Empty tree            
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>()};

            var response = InstallMethods.AddNode(tree, name, null);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success == true);
            Assert.IsNotNull(response.Tree);
            Assert.IsTrue(response.Tree.Nodes.Count() == 1);

            var firstNode = response.Tree.Nodes.FirstOrDefault();
            Assert.IsTrue(firstNode.Value.Name == name);
            Assert.IsTrue(firstNode.Value.Dependents.Count() == 0);
        }
        
        [TestMethod()]
        public void AddNode_Existing_Node()
        {
            string name = "testNode";
            // Empty tree            
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };

            var response = InstallMethods.AddNode(tree, name, null);            
            var r2 = InstallMethods.AddNode(tree, name, null);

            var firstNode = response.Tree.Nodes.FirstOrDefault();
            Assert.IsTrue(firstNode.Value.Name == name);
            Assert.IsTrue(r2.Success == true);
            Assert.IsTrue(r2.Tree.Nodes.Count() == 1);
            Assert.IsTrue(firstNode.Value.Dependents.Count() == 0);
        }

        [TestMethod()]
        public void AddNode_NewNode_DependsExisting()
        {
            string name = "testNode";
            // Empty tree            
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };

            var response = InstallMethods.AddNode(tree, name, null);            

            string dependName = "testChild";
            var r3 = InstallMethods.AddNode(tree, dependName, new List<string> { name });

            Assert.IsTrue(r3.Success == true);
            Assert.IsTrue(r3.Tree.Nodes.Count() == 2);
            Assert.IsTrue(r3.Tree.Nodes[name].Dependents.Count() == 1);
            Assert.AreSame(r3.Tree.Nodes[name].Dependents[0], r3.Tree.Nodes[dependName]);
        }

        [TestMethod()]
        public void AddNode_NewNode_DependsNew()
        {
            // Empty tree            
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };         

            string dependName = "testChild";
            string dependNotExisting = "notExist";
            var r3 = InstallMethods.AddNode(tree, dependName, new List<string> { dependNotExisting });

            Assert.IsTrue(r3.Success == true);
            Assert.IsTrue(r3.Tree.Nodes.Count() == 2);
            Assert.IsTrue(r3.Tree.Nodes[dependNotExisting].Dependents.Count() == 1);
            Assert.AreSame(r3.Tree.Nodes[dependNotExisting].Dependents[0], r3.Tree.Nodes[dependName]);
        }

        [TestMethod()]
        public void DetectDepends_True()
        {
            var NodeB = new InstallNode ("NodeB");
                       
            var NodeAChildA = new InstallNode ("NodeAChildA");
            var NodeAChildB = new InstallNode("NodeAChildB");
            NodeAChildB.Dependents.Add(NodeB);
            var NodeAChildC = new InstallNode ( "NodeAChildC");
            var NodeA = new InstallNode("NodeA");
            NodeA.Dependents.Add(NodeAChildA);
            NodeA.Dependents.Add(NodeAChildB);
            NodeA.Dependents.Add(NodeAChildC);
            var resp = InstallMethods.DetectDepends(NodeA, NodeB);
            Assert.IsTrue(resp);
        }

        [TestMethod()]
        public void DetectDepends_None_False()
        {
            var NodeB = new InstallNode("NodeB");
            var NodeC = new InstallNode("NodeC");
            var NodeAChildA = new InstallNode("NodeAChildA");
            var NodeAChildB = new InstallNode("NodeAChildB");
            NodeAChildB.Dependents.Add(NodeC);
            var NodeAChildC = new InstallNode("NodeAChildC");
            var NodeA = new InstallNode("NodeA");
            NodeA.Dependents.Add(NodeAChildA);
            NodeA.Dependents.Add(NodeAChildB);
            NodeA.Dependents.Add(NodeAChildC);
            var resp = InstallMethods.DetectDepends(NodeA, NodeB);
            Assert.IsFalse(resp);
        }

        [TestMethod()]
        public void AddNode_Depends_AddCycle_Fail()
        {
            // Empty tree            
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };

            InstallMethods.AddNode(tree, "NodeC", new List<string>() { "NodeA" });
            InstallMethods.AddNode(tree, "NodeB", new List<string>() { "NodeC" });
            var r3 =InstallMethods.AddNode(tree, "NodeA", new List<string>() { "NodeB" });                      

            Assert.IsFalse(r3.Success == true);
            Assert.IsTrue(r3.Tree.Nodes.Count() == 3);
            Assert.IsTrue(r3.Tree.Nodes["NodeA"].Dependents.Count() == 1);
            Assert.IsTrue(r3.Tree.Nodes["NodeB"].Dependents.Count() == 0);
            Assert.IsTrue(r3.Tree.Nodes["NodeC"].Dependents.Count() == 1);
        }

        public void InstallNode_Name_NotExist_Fail()
        {
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };

            InstallMethods.AddNode(tree, "NodeC", new List<string>() { "NodeA" });
            var resp = InstallMethods.InstallNodeInTree(tree, "notexist");
            Assert.IsFalse(resp.Success);
        }

        [TestMethod()]
        public void InstallTest()
        {
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };
            InstallMethods.AddNode(tree, "Http", new List<string>() { "TCP" });
            InstallMethods.AddNode(tree, "Https", new List<string>() { "TCP" });
            InstallMethods.AddNode(tree, "Chrome", new List<string>() { "Http", "Https", "GLib" });
            var resp = InstallMethods.InstallNodeInTree(tree, "Chrome");

            Assert.IsTrue(resp.Success);            
        }

        [TestMethod()]
        public void UninstallTest()
        {
            var tree = new InstallTree { Nodes = new Dictionary<string, InstallNode>() };
            InstallMethods.AddNode(tree, "Http", new List<string>() { "TCP" });
            InstallMethods.AddNode(tree, "Https", new List<string>() { "TCP" });
            InstallMethods.AddNode(tree, "Chrome", new List<string>() { "Http", "Https", "GLib" });
            var resp = InstallMethods.InstallNodeInTree(tree, "Chrome");
            var r2 = InstallMethods.UnInstallNodeInTree(tree, "Http");
            var r3 = InstallMethods.UnInstallNodeInTree(tree, "Chrome");
            var r4 = InstallMethods.UnInstallNodeInTree(tree, "TCP");
        }
    }
}