// Canonical global Story models
using System;
using System.Collections.Generic;

[Serializable]
public class StoryData
{
    public string story_name;
    public string start_node_id;
    public List<StoryNode> nodes = new List<StoryNode>();
    public StoryNode GetStartNode() => GetNodeById(start_node_id);
    public StoryNode GetNodeById(string id) => nodes != null ? nodes.Find(n => n.id == id) : null;
}

[Serializable]
public class StoryNode
{
    public string id;
    public string title;
    public string content;
    public string imagePath;
    public List<StoryChoice> choices = new List<StoryChoice>();
}

[Serializable]
public class StoryChoice
{
    public string id;
    public string text;
    public string nextId;        // legacy
    public string nextNodeId;    // alternative
    public string targetNodeId;  // preferred
    public string resultText;

    // Indicates whether this choice should be available for selection.  This property
    // is not serialized by older story JSONs, so it defaults to true when absent.
    public bool isEnabled = true;
}
