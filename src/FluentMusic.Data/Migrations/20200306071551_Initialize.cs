using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FluentMusic.Data.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: true),
                    DateModified = table.Column<DateTimeOffset>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    NeedIndex = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtworkPath = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Cover = table.Column<string>(nullable: true),
                    ArtistId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderId = table.Column<long>(nullable: true),
                    Genres = table.Column<string>(nullable: true),
                    AlbumId = table.Column<long>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    SafePath = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true),
                    FileSize = table.Column<ulong>(nullable: true),
                    BitRate = table.Column<long>(nullable: true),
                    SampleRate = table.Column<long>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TrackNumber = table.Column<long>(nullable: true),
                    TrackCount = table.Column<long>(nullable: true),
                    DiscNumber = table.Column<long>(nullable: true),
                    DiscCount = table.Column<long>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    Year = table.Column<long>(nullable: true),
                    HasLyrics = table.Column<long>(nullable: true),
                    DateAdded = table.Column<long>(nullable: false),
                    DateFileCreated = table.Column<long>(nullable: false),
                    DateLastSynced = table.Column<long>(nullable: false),
                    DateFileModified = table.Column<long>(nullable: false),
                    NeedsIndexing = table.Column<long>(nullable: true),
                    NeedsAlbumArtworkIndexing = table.Column<long>(nullable: true),
                    IndexingSuccess = table.Column<long>(nullable: true),
                    IndexingFailureReason = table.Column<string>(nullable: true),
                    Rating = table.Column<long>(nullable: true),
                    Love = table.Column<long>(nullable: true),
                    PlayCount = table.Column<long>(nullable: true),
                    SkipCount = table.Column<long>(nullable: true),
                    DateLastPlayed = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracks_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracks_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId",
                table: "Tracks",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_FolderId",
                table: "Tracks",
                column: "FolderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Artists");
        }
    }
}
