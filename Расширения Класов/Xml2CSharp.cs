using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Xml2CSharp
{
	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (Envelope)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "CardType")]
	public class CardType
	{

		[XmlElement(ElementName = "id")]
		public int Id { get; set; }

		[XmlElement(ElementName = "description")]
		public string Description { get; set; }

		[XmlElement(ElementName = "menuLevel")]
		public int MenuLevel { get; set; }
	}

	[XmlRoot(ElementName = "CardStatus")]
	public class CardStatus
	{

		[XmlElement(ElementName = "active")]
		public bool Active { get; set; }

		[XmlElement(ElementName = "description")]
		public string Description { get; set; }
	}

	[XmlRoot(ElementName = "PersonName")]
	public class PersonName
	{

		[XmlElement(ElementName = "FirstName")]
		public string FirstName { get; set; }

		[XmlElement(ElementName = "LastName")]
		public string LastName { get; set; }
	}

	[XmlRoot(ElementName = "ns4:AccountInfoList")]
	public class AccountInfoList
	{

		[XmlElement(ElementName = "ns4:Type")]
		public string Type { get; set; }

		[XmlElement(ElementName = "ns4:Code")]
		public string Code { get; set; }

		[XmlElement(ElementName = "ns4:Name")]
		public string Name { get; set; }

		[XmlElement(ElementName = "ns4:Balance")]
		public double Balance { get; set; }
	}

	[XmlRoot(ElementName = "Accounts")]
	public class Accounts
	{

		[XmlElement(ElementName = "AccountInfoList")]
		public List<AccountInfoList> AccountInfoList { get; set; }
	}

	[XmlRoot(ElementName = "CardInfoList")]
	public class CardInfoList
	{

		[XmlElement(ElementName = "CardType")]
		public string CardType { get; set; }

		[XmlElement(ElementName = "Number")]
		public int Number { get; set; }
	}

	[XmlRoot(ElementName = "Cards")]
	public class Cards
	{

		[XmlElement(ElementName = "CardInfoList")]
		public CardInfoList CardInfoList { get; set; }
	}

	[XmlRoot(ElementName = "FetchProfileResponse")]
	public class FetchProfileResponse
	{

		[XmlElement(ElementName = "ProfileID")]
		public int ProfileID { get; set; }

		[XmlElement(ElementName = "CardType")]
		public CardType CardType { get; set; }

		[XmlElement(ElementName = "CardStatus")]
		public CardStatus CardStatus { get; set; }

		[XmlElement(ElementName = "PersonName")]
		public PersonName PersonName { get; set; }

		[XmlElement(ElementName = "TextData")]
		public string TextData { get; set; }

		[XmlElement(ElementName = "HTMLData")]
		public object HTMLData { get; set; }

		[XmlElement(ElementName = "Accounts")]
		public Accounts Accounts { get; set; }

		[XmlElement(ElementName = "Vouchers")]
		public object Vouchers { get; set; }

		[XmlElement(ElementName = "Coupons")]
		public object Coupons { get; set; }

		[XmlElement(ElementName = "Cards")]
		public Cards Cards { get; set; }

		[XmlElement(ElementName = "canOpenCheck")]
		public bool CanOpenCheck { get; set; }

		[XmlAttribute(AttributeName = "ns5")]
		public string Ns5 { get; set; }

		[XmlAttribute(AttributeName = "ns2")]
		public string Ns2 { get; set; }

		[XmlAttribute(AttributeName = "ns4")]
		public string Ns4 { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Body")]
	public class Body
	{

		[XmlElement(ElementName = "FetchProfileResponse")]
		public FetchProfileResponse FetchProfileResponse { get; set; }
	}

	[XmlRoot(ElementName = "Envelope")]
	public class Envelope
	{

		[XmlElement(ElementName = "Header")]
		public object Header { get; set; }

		[XmlElement(ElementName = "Body")]
		public Body Body { get; set; }

		[XmlAttribute(AttributeName = "SOAP-ENV")]
		public string SOAPENV { get; set; }

		[XmlText]
		public string Text { get; set; }
	}



}
