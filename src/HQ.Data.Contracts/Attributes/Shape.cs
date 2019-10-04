#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

namespace HQ.Data.Contracts.Attributes
{
	/// <summary>
	///     See: https://www.graphviz.org/doc/info/shapes.html
	/// </summary>
	public enum Shape : byte
	{
		None,
		Box,
		Polygon,
		Ellipse,
		Oval,
		Circle,
		Point,
		Egg,
		Triangle,
		Plain,
		Diamond,
		Trapezium,
		Parallelogram,
		House,
		Pentagon,
		Hexagon,
		Septagon,
		Octagon,
		DoubleCircle,
		DoubleOctagon,
		TripleOctagon,
		InvTriangle,
		InvTrapezium,
		InvHouse,
		MDiamond,
		MSquare,
		MCircle,
		Square,
		Star,
		Underline,
		Cylinder,
		Note,
		Tab,
		Folder,
		Box3d,
		Component,
		Promoter,
		Cds,
		Terminator,
		Utr,
		PrimerSite,
		RestrictionSite,
		FivePOverhang,
		ThreePOverhang,
		NOverhang,
		Assembly,
		Signature,
		Insulator,
		RiboSite,
		RnaStab,
		ProteaseSite,
		ProteinStab,
		RPromoter,
		RArrow,
		LArrow,
		LPromoter,

		Rectangle = Box,
		Rect = Rectangle,
		Plaintext = None
	}
}