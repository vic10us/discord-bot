using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using bot.Features.Games;
using bot.Features.MemeGenerator;
using bot.Features.MondayQuotes;
using MongoDB.Driver;
using Svg;
using Xunit;
using Xunit.Abstractions;

namespace tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task CanGetMondayQuote()
        {
            var qs = new MondayQuotesService();
            var quote = await qs.GetQuote();
            Assert.NotEmpty(quote);
        }
        
        [Fact]
        public void MemeFactoryTwitterMediaDestination()
        {
            var twd = new TwitterMediaDestination()
            {
                MemeText = "“When people go to work, they shouldn’t leave their hearts at home.” – Betty Bender"
            };
            var mf = new MemeFactory<TwitterMediaDestination>(twd);
            mf.Generate();
            Assert.True(true);
        }

        [Fact]
        public void CanCreateSVG()
        {
            var svg = File.ReadAllText(@"C:\Users\sherl\OneDrive\Desktop\rank.svg");
            var svgDocument = SvgDocument.FromSvg<SvgDocument>(svg);
            svgDocument.ShapeRendering = SvgShapeRendering.GeometricPrecision;
            var bitmap = svgDocument.Draw();
            bitmap.Save(@"C:\Users\sherl\OneDrive\Desktop\rank.png", ImageFormat.Png);
        }

        [Fact]
        public void CanListMongoDatabases()
        {
            var dbClient = new MongoClient("mongodb://localhost");
            var dbList = dbClient.ListDatabases().ToList();
            foreach (var db in dbList)
            {
                _testOutputHelper.WriteLine(db.ToString());
            }
        }

        [Fact]
        public async Task CanCallConvertImageApi()
        {
            var fileName = @"C:\Users\sherl\OneDrive\Desktop\rank.svg";
            var image = File.OpenRead(fileName);
            using var client = new HttpClient();
            using var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            var imageContent = new StreamContent(image);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(fileName));
            content.Add(imageContent, "image", "rank.svg");
            content.Add(new StringContent("144"), "dpi");
            content.Add(new StringContent("true"), "keep");
            using var message =
                await client.PostAsync("http://localhost:3000/images/conversions", content);
            var input = await message.Content.ReadAsByteArrayAsync();
            
            await File.WriteAllBytesAsync(@$"C:\Users\sherl\OneDrive\Desktop\{DateTimeOffset.Now.UtcTicks}-rank.png", input);
        }

        private static ulong TotalXpForLevel(ulong x) => (ulong)(5.0f / 6.0f * x * (x + 7.0f) * (2.0f * x + 13.0f));

        private static ulong LevelForTotalXp(ulong totalXp)
        {
            var lvl = (ulong)0;
            var totalXpForCurrentLevel = TotalXpForLevel(lvl+1);
            while (totalXp >= totalXpForCurrentLevel)
            {
                lvl++;
                totalXpForCurrentLevel = TotalXpForLevel(lvl+1);
            }
            return lvl;
        }
        
        private static ulong ALevelForTotalXp(ulong totalXp) => totalXp >= 100 ? 
            (ulong)(0.14057f * Math.Pow(1.7321f * Math.Sqrt(3888.0f * Math.Pow(totalXp, 2) + 291600.0f * totalXp - 207025.0f) + 108.0f * totalXp + 4050.0f, 1.0f / 3.0f) - 4.5000f) + 1 
            : 0;

        [Fact]
        public void CanRemoveXp()
        {
            ulong lvl = 13;
            ulong xp = 1550;
            ulong totalLevelXp = TotalXpForLevel(13);
            ulong totalXp = totalLevelXp + xp;
            ulong lvlForTotalXp = LevelForTotalXp(totalXp); 
            _testOutputHelper.WriteLine($"Lvl: {lvl}, xp: {xp}, TotalLevelXp: {totalLevelXp}, TotalXp: {totalXp}, lvlForTotalXp: {lvlForTotalXp}");
            totalXp -= 500;
            lvl = LevelForTotalXp(totalXp);
            totalLevelXp = TotalXpForLevel(lvl);
            xp = totalXp - totalLevelXp;
            _testOutputHelper.WriteLine($"Lvl: {lvl}, xp: {xp}, TotalLevelXp: {totalLevelXp}, TotalXp: {totalXp}");
        }
        
        [Theory]
        [InlineData(1, 100)]
        [InlineData(2, 255)]
        [InlineData(3, 475)]
        [InlineData(4, 769)]
        [InlineData(5, 1150)]
        [InlineData(6, 1625)]
        [InlineData(7, 2205)]
        [InlineData(8, 2900)]
        [InlineData(9, 3720)]
        [InlineData(10, 4674)]
        [InlineData(11, 5774)]
        [InlineData(12, 7030)]
        [InlineData(13, 8450)]
        [InlineData(13, 10000)]
        [InlineData(14, 10044)]
        [InlineData(15, 11825)]
        [InlineData(16, 13800)]
        [InlineData(17, 15980)]
        [InlineData(18, 18375)]
        [InlineData(19, 20995)]
        [InlineData(20, 23849)]
        [InlineData(21, 26950)]
        [InlineData(22, 30304)]
        [InlineData(23, 33925)]
        [InlineData(24, 37820)]
        [InlineData(25, 41999)]
        [InlineData(26, 46475)]
        [InlineData(27, 51255)]
        [InlineData(28, 56349)]
        [InlineData(29, 61770)]
        [InlineData(30, 67525)]
        [InlineData(31, 73625)]
        [InlineData(32, 80080)]
        [InlineData(33, 86900)]
        [InlineData(34, 94095)]
        [InlineData(35, 101675)]
        [InlineData(36, 109650)]
        [InlineData(37, 118030)]
        [InlineData(38, 126825)]
        [InlineData(39, 136045)]
        [InlineData(40, 145700)]
        [InlineData(41, 155799)]
        [InlineData(42, 166355)]
        [InlineData(43, 177375)]
        [InlineData(44, 188869)]
        [InlineData(45, 200850)]
        [InlineData(46, 213325)]
        [InlineData(47, 226304)]
        [InlineData(48, 239800)]
        [InlineData(49, 253819)]
        [InlineData(50, 268374)]
        [InlineData(51, 283475)]
        [InlineData(52, 299129)]
        [InlineData(53, 315349)]
        [InlineData(54, 332145)]
        [InlineData(55, 349524)]
        [InlineData(56, 367499)]
        [InlineData(57, 386080)]
        [InlineData(58, 405274)]
        [InlineData(59, 425094)]
        [InlineData(60, 445550)]
        [InlineData(61, 466649)]
        [InlineData(62, 488404)]
        [InlineData(63, 510825)]
        [InlineData(64, 533920)]
        [InlineData(65, 557699)]
        [InlineData(66, 582175)]
        [InlineData(67, 607355)]
        [InlineData(68, 633250)]
        [InlineData(69, 659870)]
        [InlineData(70, 687225)]
        [InlineData(71, 715325)]
        [InlineData(72, 744180)]
        [InlineData(73, 773800)]
        [InlineData(74, 804195)]
        [InlineData(75, 835375)]
        [InlineData(76, 867350)]
        [InlineData(77, 900130)]
        [InlineData(78, 933725)]
        [InlineData(79, 968144)]
        [InlineData(80, 1003400)]
        [InlineData(81, 1039500)]
        [InlineData(82, 1076454)]
        [InlineData(83, 1114275)]
        [InlineData(84, 1152970)]
        [InlineData(85, 1192549)]
        [InlineData(86, 1233025)]
        [InlineData(87, 1274405)]
        [InlineData(88, 1316699)]
        [InlineData(89, 1359920)]
        [InlineData(90, 1404075)]
        [InlineData(91, 1449174)]
        [InlineData(92, 1495229)]
        [InlineData(93, 1542250)]
        [InlineData(94, 1590244)]
        [InlineData(95, 1639224)]
        [InlineData(96, 1689200)]
        [InlineData(97, 1740179)]
        [InlineData(98, 1792175)]
        [InlineData(99, 1845195)]
        [InlineData(100, 1899249)]
        public void CanGetLevelFroTotalXp(ulong expectedLevel, ulong totalXp)
        {
            var x = LevelForTotalXp(totalXp);
            Assert.Equal(expectedLevel, x);
            _testOutputHelper.WriteLine($"{totalXp}xp = Level {x}");
        }
        
        [Theory]
        [InlineData(1, 100)]
        [InlineData(2, 255)]
        [InlineData(3, 475)]
        [InlineData(4, 769)]
        [InlineData(5, 1150)]
        [InlineData(6, 1625)]
        [InlineData(7, 2205)]
        [InlineData(8, 2900)]
        [InlineData(9, 3720)]
        [InlineData(10, 4674)]
        [InlineData(11, 5774)]
        [InlineData(12, 7030)]
        [InlineData(13, 8450)]
        [InlineData(14, 10044)]
        [InlineData(15, 11825)]
        [InlineData(16, 13800)]
        [InlineData(17, 15980)]
        [InlineData(18, 18375)]
        [InlineData(19, 20995)]
        [InlineData(20, 23849)]
        [InlineData(21, 26950)]
        [InlineData(22, 30304)]
        [InlineData(23, 33925)]
        [InlineData(24, 37820)]
        [InlineData(25, 41999)]
        [InlineData(26, 46475)]
        [InlineData(27, 51255)]
        [InlineData(28, 56349)]
        [InlineData(29, 61770)]
        [InlineData(30, 67525)]
        [InlineData(31, 73625)]
        [InlineData(32, 80080)]
        [InlineData(33, 86900)]
        [InlineData(34, 94095)]
        [InlineData(35, 101675)]
        [InlineData(36, 109650)]
        [InlineData(37, 118030)]
        [InlineData(38, 126825)]
        [InlineData(39, 136045)]
        [InlineData(40, 145700)]
        [InlineData(41, 155799)]
        [InlineData(42, 166355)]
        [InlineData(43, 177375)]
        [InlineData(44, 188869)]
        [InlineData(45, 200850)]
        [InlineData(46, 213325)]
        [InlineData(47, 226304)]
        [InlineData(48, 239800)]
        [InlineData(49, 253819)]
        [InlineData(50, 268374)]
        [InlineData(51, 283475)]
        [InlineData(52, 299129)]
        [InlineData(53, 315349)]
        [InlineData(54, 332145)]
        [InlineData(55, 349524)]
        [InlineData(56, 367499)]
        [InlineData(57, 386080)]
        [InlineData(58, 405274)]
        [InlineData(59, 425094)]
        [InlineData(60, 445550)]
        [InlineData(61, 466649)]
        [InlineData(62, 488404)]
        [InlineData(63, 510825)]
        [InlineData(64, 533920)]
        [InlineData(65, 557699)]
        [InlineData(66, 582175)]
        [InlineData(67, 607355)]
        [InlineData(68, 633250)]
        [InlineData(69, 659870)]
        [InlineData(70, 687225)]
        [InlineData(71, 715325)]
        [InlineData(72, 744180)]
        [InlineData(73, 773800)]
        [InlineData(74, 804195)]
        [InlineData(75, 835375)]
        [InlineData(76, 867350)]
        [InlineData(77, 900130)]
        [InlineData(78, 933725)]
        [InlineData(79, 968144)]
        [InlineData(80, 1003400)]
        [InlineData(81, 1039500)]
        [InlineData(82, 1076454)]
        [InlineData(83, 1114275)]
        [InlineData(84, 1152970)]
        [InlineData(85, 1192549)]
        [InlineData(86, 1233025)]
        [InlineData(87, 1274405)]
        [InlineData(88, 1316699)]
        [InlineData(89, 1359920)]
        [InlineData(90, 1404075)]
        [InlineData(91, 1449174)]
        [InlineData(92, 1495229)]
        [InlineData(93, 1542250)]
        [InlineData(94, 1590244)]
        [InlineData(95, 1639224)]
        [InlineData(96, 1689200)]
        [InlineData(97, 1740179)]
        [InlineData(98, 1792175)]
        [InlineData(99, 1845195)]
        [InlineData(100, 1899249)]
        public void TestXp(ulong level, ulong expectedXp)
        {
            Assert.Equal(expectedXp, TotalXpForLevel(level));
        }

        private static (int, int, int) ComputeLevelAndXp(int lvl, int xp, Action<string> cb = null)
        {
            while (xp > 5 * Math.Pow(lvl, 2) + 50 * lvl + 100)
            {
                cb?.Invoke($"New Level: {lvl+1} -{(5 * Math.Pow(lvl, 2) + 50 * lvl + 100)} xp ({xp-(5 * Math.Pow(lvl, 2) + 50 * lvl + 100)})");
                xp -= (int)(5 * Math.Pow(lvl, 2) + 50 * lvl + 100);
                lvl++;
            }
            var next = (int)(5 * Math.Pow(lvl, 2) + 50 * lvl + 100);
            return (lvl, xp, next);
        }

        [Fact]
        public void CanComputeNext()
        {
            var (l, x, _) = (0, 256, 0);
            var (lvl, xp, next) = ComputeLevelAndXp(l, x, (xxx) =>
            {
                _testOutputHelper.WriteLine(xxx);
            });
            var totalXp = xp;
            for (var i = 0; i < lvl; i++)
            {
                totalXp += (int)(5 * Math.Pow(i, 2) + 50 * i + 100);
            }
            _testOutputHelper.WriteLine($"Level: {lvl} Curr: {xp} Next: {next} Total: {totalXp}");
        }

        [Fact]
        public void CanComputeXpForNextLevel()
        {
            var lvl = 0;
            var xp = 10110;
            while (xp > 5 * Math.Pow(lvl, 2) + 50 * lvl + 100)
            {
                _testOutputHelper.WriteLine($"New Level: {lvl+1} -{(5 * Math.Pow(lvl, 2) + 50 * lvl + 100)} xp ({xp-(5 * Math.Pow(lvl, 2) + 50 * lvl + 100)})");
                xp -= (int)(5 * Math.Pow(lvl, 2) + 50 * lvl + 100);
                lvl++;
            }
            var totalXp = xp;
            for (var l = 0; l < lvl; l++)
            {
                totalXp += (int)(5 * Math.Pow(l, 2) + 50 * l + 100);
            }
            var next = 5 * Math.Pow(lvl, 2) + 50 * lvl + 100;
            _testOutputHelper.WriteLine($"Level: {lvl} Curr: {xp} Next: {next} Total: {totalXp}");
        }

        public static IEnumerable<object[]> RollCountData => Enumerable.Range(0, 1000).Select(x => new object[] { (object)x }).ToArray();

        [Theory]
        [MemberData(nameof(RollCountData))]
        public void CanGetNextRoll(int iteration)
        {
            var dc = new DiceGame();
            var nextRoll = dc.GetNextRoll(6);
            _testOutputHelper.WriteLine($"[{iteration}]Rolled: {nextRoll}");
            Assert.True(nextRoll > 0 && nextRoll <= 6);
        }

        [Theory]
        [MemberData(nameof(RollCountData))]
        public void CanGetNextRolls(int iteration)
        {
            var dc = new DiceGame();
            var nextRolls = dc.GetNextRolls();
            var nextRollsDelim = nextRolls.Select(v => $"{v}").Aggregate((o, n) => $"{o}, {n}");
            _testOutputHelper.WriteLine($"[{iteration}]Rolls: {nextRollsDelim}, Sum: {nextRolls.Sum(v => v)}");
            Assert.True(nextRolls.Count == 2 && nextRolls.All(v => v > 0 && v <= 6) && nextRolls.Sum(v => v) <= 12);
        }
    }
}