# Overview

Graph Tools Foundation aims to provide an easy-to-use and flexible
framework to build graph based tools. It provides customizable
user interface elements to represent the graph. The elements update
themselves when the underlying model changes. When the user interacts
with the UI, it triggers commands that modify the underlying model.


The main parts of a GTF window are the
[`GraphView`](graphview-overview.md), where the graph is drawn using nodes
and edges, the [`Blackboard`](blackboard-overview.md), used to define
the inputs and outputs of a graph, the `Minimap`, which provides
an view of the whole graph and can be used to navigate large graphs, and
the local node inspector, used to edit the properties of selected nodes.
All these elements are what we call *views*, which are 
`VisualElement`s that observe the state. To do so, they are associated with
a `StateObserver`. The observer declares interest in some part of the state
