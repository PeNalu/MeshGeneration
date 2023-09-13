using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdge
{
    //The vertex the edge points to
    public Vertex v;

    //The face this edge is a part of
    public Triangle t;

    //The next edge
    public HalfEdge nextEdge;
    //The previous
    public HalfEdge prevEdge;
    //The edge going in the opposite direction
    public HalfEdge oppositeEdge;

    //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
    //and a face (triangle) class with a reference to a half edge which is a part of this face 
    public HalfEdge(Vertex v)
    {
        this.v = v;
    }

	//Flip an edge
	public static void FlipEdge(HalfEdge one)
	{
		//The data we need
		//This edge's triangle
		HalfEdge two = one.nextEdge;
		HalfEdge three = one.prevEdge;
		//The opposite edge's triangle
		HalfEdge four = one.oppositeEdge;
		HalfEdge five = one.oppositeEdge.nextEdge;
		HalfEdge six = one.oppositeEdge.prevEdge;
		//The vertices
		Vertex a = one.v;
		Vertex b = one.nextEdge.v;
		Vertex c = one.prevEdge.v;
		Vertex d = one.oppositeEdge.nextEdge.v;

		//Flip
		//Change vertex
		a.halfEdge = one.nextEdge;
		c.halfEdge = one.oppositeEdge.nextEdge;

		//Change half-edge
		//Half-edge - half-edge connections
		one.nextEdge = three;
		one.prevEdge = five;

		two.nextEdge = four;
		two.prevEdge = six;

		three.nextEdge = five;
		three.prevEdge = one;

		four.nextEdge = six;
		four.prevEdge = two;

		five.nextEdge = one;
		five.prevEdge = three;

		six.nextEdge = two;
		six.prevEdge = four;

		//Half-edge - vertex connection
		one.v = b;
		two.v = b;
		three.v = c;
		four.v = d;
		five.v = d;
		six.v = a;

		//Half-edge - triangle connection
		Triangle t1 = one.t;
		Triangle t2 = four.t;

		one.t = t1;
		three.t = t1;
		five.t = t1;

		two.t = t2;
		four.t = t2;
		six.t = t2;

		//Opposite-edges are not changing!

		//Triangle connection
		t1.v1 = b;
		t1.v2 = c;
		t1.v3 = d;

		t2.v1 = b;
		t2.v2 = d;
		t2.v3 = a;

		t1.halfEdge = three;
		t2.halfEdge = four;
	}
}