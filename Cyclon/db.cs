using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
namespace Cyclon
{
    internal class TestContext : DbContext
    {
        // テストテーブル
        public DbSet<TTestData> TestData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder stringBuilder = new()
            {
                DataSource = "Test.sqlite3",
            };
            using SqliteConnection sqliteConnection = new(stringBuilder.ToString());
            optionsBuilder.UseSqlite(sqliteConnection);
        }
    }

    [Table("t_test")]
    [Index(nameof(Name), IsUnique = true)]
    internal class TTestData
    {
        // ID
        [Key]
        [Column("test_id")]
        public Int32 Id { get; set; }

        // 氏名
        [Column("test_name")]
        public String Name { get; set; } = String.Empty;

        // 身長
        [Column("test_height")]
        public Double? Height { get; set; }
    }
}
