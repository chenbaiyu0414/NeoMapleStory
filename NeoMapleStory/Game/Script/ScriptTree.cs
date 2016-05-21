using System;
using System.Collections.Generic;
using NeoMapleStory.Game.Script.NPC;

namespace NeoMapleStory.Game.Script
{
    public class ScriptTree
    {
        public ScriptTree(Action<NpcConversationManager> data)
        {
            Data = data;
        }

        public ScriptTree ParentNode { get; set; }
        public Action<NpcConversationManager> Data { get; set; }
        public List<ScriptTree> ChildrenNodes { get; set; } = new List<ScriptTree>();

        public ScriptTree AddNode(ScriptTree node)
        {
            if (!ChildrenNodes.Contains(node))
            {
                node.ParentNode = this;
                ChildrenNodes.Add(node);
            }
            return this;
        }

        public ScriptTree AddNodes(ScriptTree[] nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }
            return this;
        }

        public void RemoveAll() => ChildrenNodes.Clear();
    }

    public class Test
    {
        private ScriptTree m_currentNode;
        private readonly ScriptTree m_tree;

        public Test()
        {
            m_tree = new ScriptTree(ss => { ss.SendChoice("你好啊!\r\n#L0#kkk#l\r\n#L1#lalal#l"); }).AddNodes(new[]
            {
                new ScriptTree(cm => { cm.SendYesNo("真的吗"); }).AddNodes(new[]
                {
                    new ScriptTree(cm =>
                    {
                        cm.SendOk("真的");
                        cm.Close();
                    }),
                    new ScriptTree(cm =>
                    {
                        cm.SendOk("假的");
                        cm.Close();
                    })
                }),
                new ScriptTree(cm =>
                {
                    cm.SendOk("拜拜");
                    cm.Close();
                })
            });
        }

        public void Start(byte isCountinue, byte hasInput, int selection)
        {
            var cm = new NpcConversationManager(null, 1);

            if (isCountinue == 1 && selection == 0)
                if (m_currentNode == null)
                    m_currentNode = m_tree;
                else if (m_currentNode.ChildrenNodes.Count > 0)
                    m_currentNode = m_currentNode.ChildrenNodes[selection];
                else
                    m_currentNode = m_currentNode.ParentNode ?? m_currentNode;

            m_currentNode.Data(cm);
        }
    }
}