using System;
using System.Globalization;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol.Test
{
	[TestFixture]
	public class VssHistoryParserTest : CustomAssertion
	{
		VssHistoryParser _parser;

		[SetUp]
		public void SetUp()
		{
			_parser = new VssHistoryParser(new EnglishVssLocale());
		}

		/*
		public void AssertEquals(Modification expected, Modification actual)
		{
			Assert.AreEqual(expected.Comment, actual.Comment);
			Assert.AreEqual(expected.EmailAddress, actual.EmailAddress);
			Assert.AreEqual(expected.FileName, actual.FileName);
			Assert.AreEqual(expected.FolderName, actual.FolderName);
			Assert.AreEqual(expected.ModifiedTime, actual.ModifiedTime);
			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.UserName, actual.UserName);
			Assert.AreEqual(expected, actual);
		}
		*/

		[Test]
		public void Parse()
		{
			Modification[] mods = _parser.Parse(VssMother.ContentReader, VssMother.OLDEST_ENTRY, VssMother.NEWEST_ENTRY);
			Assert.IsNotNull(mods, "mods should not be null");
			Assert.AreEqual(19, mods.Length);			
		}

		[Test]
		public void ReadAllEntriesTest() 
		{
			string[] entries = _parser.ReadAllEntries(VssMother.ContentReader);
			Assert.AreEqual(24, entries.Length);
		}

		[Test]
		public void IsEntryDelimiter()
		{
			string line = "*****  cereal.txt  *****";
			Assert.IsTrue(_parser.IsEntryDelimiter(line), "should recognize as delim");

			line = "*****************  Version 8   *****************";
			Assert.IsTrue(_parser.IsEntryDelimiter(line), "should recognize as delim");

			line = "*****";
			Assert.IsTrue(_parser.IsEntryDelimiter(line) == false, string.Format("should not recognize as delim '{0}'", line));

			line = "*****************  Version 4   *****************";
			Assert.IsTrue(_parser.IsEntryDelimiter(line), "should recognize as delim");
		}

		[Test]
		public void ParseCreatedModification()
		{
			string entry = EntryWithSingleLineComment();
			
			Modification expected = new Modification();
			expected.Comment = "added subfolder";
			expected.UserName = "Admin";
			expected.ModifiedTime = new DateTime(2002, 9, 16, 14, 41, 0);
			expected.Type = "Created";
			expected.FileName = "[none]";
			expected.FolderName = "plant";

			Modification[] actual = _parser.parseModifications(makeArray(entry));
			Assert.IsNotNull(actual, "expected a mod");
			Assert.AreEqual(0, actual.Length, "created should not have produced a modification");
		}

		[Test]
		public void ParseUsernameAndUSDate()
		{
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Admin        Date:  9/16/02   Time:  2:40p\r\n";
			CheckInParser parser = new CheckInParser(line, new EnglishVssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Admin";
			DateTime expectedDate = new DateTime(2002, 09, 16, 14, 40, 0);
			Assert.AreEqual(expectedUsername, mod.UserName);
			Assert.AreEqual(expectedDate, mod.ModifiedTime);
		}

		[Test]
		public void ParseUsernameAndUKDate()
		{
			Modification mod = new Modification();
			string line = "foo\r\nUser: Admin        Date:  16/9/02   Time:  22:40\r\n";
			CheckInParser parser = new CheckInParser(line, new EnglishVssLocale(new CultureInfo("en-GB")));
			parser.ParseUsernameAndDate(mod);

			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 9, 16, 22, 40, 0), mod.ModifiedTime);
		}

		[Test]
		public void ParseUsernameAndFRDate()
		{
			Modification mod = new Modification();
			string line = "foo\r\nUtilisateur: Admin        Date:  2/06/04   Heure: 14:04\r\n";
			CheckInParser parser = new CheckInParser(line, new FrenchVssLocale());
			parser.ParseUsernameAndDate(mod);

			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2004,6,2,14,4,0), mod.ModifiedTime);
		}

		[Test]
		public void ParseUsernameAndDateWithPeriod() 
		{
			//User: Gabriel.gilabert     Date:  5/08/03   Time:  4:06a
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Gabriel.gilabert     Date:  5/08/03   Time:  4:06a\r\n";
			CheckInParser parser = new CheckInParser(line, new EnglishVssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Gabriel.gilabert";
			DateTime expectedDate = new DateTime(2003, 05, 08, 04, 06, 0);
			Assert.AreEqual(expectedUsername, mod.UserName);
			Assert.AreEqual(expectedDate, mod.ModifiedTime);
		}
		
		[Test]
		public void ParseMultiWordUsername()
		{
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Gabriel Gilabert     Date:  5/08/03   Time:  4:06a\r\n";
			CheckInParser parser = new CheckInParser(line, new EnglishVssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Gabriel Gilabert";
			DateTime expectedDate = new DateTime(2003, 05, 08, 04, 06, 0);
			Assert.AreEqual(expectedUsername, mod.UserName);
			Assert.AreEqual(expectedDate, mod.ModifiedTime);
		}

		[Test, ExpectedException(typeof(CruiseControlException))]
		public void ParseInvalidUsernameLine()
		{
			string line = "foo\r\nbar\r\n";
			new CheckInParser(line, new EnglishVssLocale(new CultureInfo("en-US"))).ParseUsernameAndDate(new Modification());
		}

		[Test]
		public void ParseFileName() 
		{
			string fileName = "**** Im a file name.fi     ********\r\n jakfakjfnb  **** ** lkjnbfgakj ****";
			CheckInParser parser = new CheckInParser(fileName, new EnglishVssLocale(new CultureInfo("en-US")));
			string actual = parser.ParseFileName();
			Assert.AreEqual("Im a file name.fi", actual);
		}

		[Test]
		public void ParseFileAndFolder_checkin()
		{
			string entry = @"*****  happyTheFile.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/folders/i/got/em
Comment: added fir to tree file, checked in recursively from project root";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 9, 16, 17, 01, 0), mod.ModifiedTime);
			Assert.AreEqual("Checked in", mod.Type);
			Assert.AreEqual("added fir to tree file, checked in recursively from project root",mod.Comment);
		}

		[Test]
		public void ParseFileAndFolderFR_checkin()
		{
			// change the parser culture for this test only
			_parser = new VssHistoryParser(new FrenchVssLocale());

			string entry = @"*****  happyTheFile.txt  *****
Version 16
Utilisateur: Admin        Date:  25/11/02   Heure: 17:32
Archiv� dans $/you/want/folders/i/got/em
Commentaire: adding this file makes me so happy";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 11, 25, 17, 32, 0), mod.ModifiedTime);
			Assert.AreEqual("Archiv� dans", mod.Type);
			Assert.AreEqual("adding this file makes me so happy",mod.Comment);
		}

		[Test]
		public void ParseFileAndFolderWithNoComment()
		{
			string entry = @"*****  happyTheFile.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/folders/i/got/em
";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, "happyTheFile.txt", "$/you/want/folders/i/got/em");
			Assert.AreEqual("Checked in", mod.Type);
			Assert.IsNull(mod.Comment);
		}

		[Test]
		public void ParseFileAndFolder_addAtRoot()
		{
			// note: this represents the entry after version line insertion 
			// (see _parser.InsertVersionLine)
			string entry = @"*****************  Version 2   *****************
Version 2
User: Admin        Date:  9/16/02   Time:  2:40p
happyTheFile.txt added
";
			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "[projectRoot]";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 9, 16, 14, 40, 0), mod.ModifiedTime);
			Assert.AreEqual("added", mod.Type);
			Assert.AreEqual(null, mod.Comment);
		}
		
		[Test]
		public void ParseFileAndFolderIfFolderIsCalledAdded()
		{
			string entry = @"*****  added  *****
Version 8
User: Admin        Date:  9/16/02   Time:  5:29p
happyTheFile.txt added
";
			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "added";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 9, 16, 17, 29, 0), mod.ModifiedTime);
			Assert.AreEqual("added", mod.Type);
			Assert.AreEqual(null, mod.Comment);
		}

		[Test]
		public void ParseFileAndFolder_deleteFromSubfolder()
		{
string entry = @"*****  iAmAFolder  *****
Version 8
User: Admin        Date:  9/16/02   Time:  5:29p
happyTheFile.txt deleted";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "iAmAFolder";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.AreEqual("Admin", mod.UserName);
			Assert.AreEqual(new DateTime(2002, 9, 16, 17, 29, 0), mod.ModifiedTime);
			Assert.AreEqual("deleted", mod.Type);
			Assert.AreEqual(null, mod.Comment);
		}

		private string[] makeArray(params string[] entries) 
		{
			return entries;
		}

		private Modification ParseAndAssertFilenameAndFolder(
			string entry, string expectedFile, string expectedFolder)
		{
			string[] entries = makeArray(entry);

			Modification[] mod = _parser.parseModifications(entries);
			
			Assert.IsNotNull(mod);
			Assert.AreEqual(1, mod.Length);
			Assert.AreEqual(expectedFile, mod[0].FileName);
			Assert.AreEqual(expectedFolder, mod[0].FolderName);

			return mod[0];
		}

		[Test]
		public void ParseSingleLineComment()
		{
			CheckInParser parser = new CheckInParser(EntryWithSingleLineComment(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual("added subfolder", mod.Comment);
		}

		[Test]
		public void ParseMultiLineComment()
		{
			CheckInParser parser = new CheckInParser(EntryWithMultiLineComment(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual(@"added subfolder
and then added a new line", mod.Comment);
		}

		[Test]
		public void ParseEmptyComment()
		{
			CheckInParser parser = new CheckInParser(EntryWithEmptyComment(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual(String.Empty, mod.Comment);
		}

		[Test]
		public void ParseEmptyLineComment()
		{
			CheckInParser parser = new CheckInParser(EntryWithEmptyCommentLine(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual(null, mod.Comment);
		}

		[Test]
		public void ParseNoComment()
		{
			CheckInParser parser = new CheckInParser(EntryWithNoCommentLine(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual(null, mod.Comment);
		}

		[Test]
		public void ParseNonCommentAtCommentLine()
		{
			CheckInParser parser = new CheckInParser(EntryWithNonCommentAtCommentLine(), new EnglishVssLocale());
			Modification mod = new Modification();
			parser.ParseComment(mod);
			Assert.AreEqual(null, mod.Comment);
		}

		private string EntryWithSingleLineComment()
		{
			string entry = 
				@"*****  plant  *****
Version 1
User: Admin        Date:  9/16/02   Time:  2:41p
Created
Comment: added subfolder";
			return entry;
		}

		private string EntryWithMultiLineComment()
		{
			return @"*****  plant  *****
Version 1
User: Admin        Date:  9/16/02   Time:  2:41p
Created
Comment: added subfolder
and then added a new line";
		}

		private string EntryWithEmptyComment()
		{
return @"*****************  Version 1   *****************
User: Admin        Date:  9/16/02   Time:  2:29p
Created
Comment: 

";
		}

		private string EntryWithEmptyCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added

";
		}

		private string EntryWithNoCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added";
		}
			
		private string EntryWithNonCommentAtCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added
booya, grandma, booya";
		}
	}
}
