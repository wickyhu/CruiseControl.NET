using System;
using System.Globalization;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Util;
using Exortech.NetReflector;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol.Test
{
	[TestFixture]
	public class StarTeamTest : CustomAssertion
	{
		public const string ST_XML =
			@"<sourceControl type=""starteam"">
				<executable>..\tools\starteam\stcmd.exe</executable>
				<username>Admin</username>
				<password>admin</password>
				<host>10.1.1.64</host>
				<port>49201</port>
				<project>.NET LAB</project>
				<path>CC.NET/starteam-ccnet</path>				
			</sourceControl>";	

		private StarTeam _starteam;

		[SetUp]
		protected void SetUp()
		{
			_starteam = CreateStarTeam();
		}
		
		[Test]
		public void TestCreateHistoryProcess()
		{				
			DateTime from = new DateTime(2001, 1, 21, 20, 0, 0);
			DateTime to = new DateTime(2002, 2, 22, 20, 0, 0);

			ProcessInfo actual = _starteam.CreateHistoryProcessInfo(from, to);			

			string expectedExecutable = @"..\tools\starteam\stcmd.exe";
			string expectedArgs = "hist -nologo -x -is -filter IO -p \"Admin:admin@10.1.1.64:49201/.NET LAB/CC.NET/starteam-ccnet\" \"*\"";

			Assert.IsNotNull(actual);
			Assert.AreEqual(expectedExecutable, actual.FileName);
			Assert.AreEqual(expectedArgs, actual.Arguments);
		}
		
		[Test]
		public void TestValuesSet()
		{			
			Assert.AreEqual(@"..\tools\starteam\stcmd.exe", _starteam.Executable);
			Assert.AreEqual("Admin", _starteam.Username);
			Assert.AreEqual("admin", _starteam.Password);
			Assert.AreEqual("10.1.1.64", _starteam.Host);
			Assert.AreEqual(49201, _starteam.Port);
			Assert.AreEqual(".NET LAB", _starteam.Project);
			Assert.AreEqual("CC.NET/starteam-ccnet", _starteam.Path);
			
		}

		[Test]
		public void TestFormatDate()
		{
			_starteam.Culture = new CultureInfo("en-US");
			DateTime date = new DateTime(2002, 2, 22, 20, 0, 0);
//			string expected = "02/22/2002 08:00:00 PM";
			string expected = "2/22/2002 8:00:00 PM";
			string actual = _starteam.FormatCommandDate(date);
			Assert.AreEqual(expected, actual);

			date = new DateTime(2002, 2, 22, 12, 0, 0);
//			expected = "02/22/2002 12:00:00 PM";
			expected = "2/22/2002 12:00:00 PM";
			actual = _starteam.FormatCommandDate(date);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestExecutable_default()
		{
			Assert.AreEqual("stcmd.exe", new StarTeam().Executable);
		}

		[Test]
		public void TestHost_default()
		{
			Assert.AreEqual("127.0.0.1", new StarTeam().Host);
		}

		[Test]
		public void TestPort_default()
		{
			Assert.AreEqual(49201, new StarTeam().Port);
		}

		[Test]
		public void TestPath_default()
		{
			Assert.AreEqual(String.Empty, new StarTeam().Path);
		}
		
		private StarTeam CreateStarTeam()
		{
			StarTeam starTeam = new StarTeam();
			NetReflector.Read(ST_XML, starTeam);
			return starTeam;
		}
	} 	
}