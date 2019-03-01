using Microsoft.EntityFrameworkCore.Migrations;

namespace Magentaize.FluentPlayer.Data.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumArtworks",
                columns: table => new
                {
                    AlbumArtworkId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtworkId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumArtworks", x => x.AlbumArtworkId);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    FolderId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: true),
                    SafePath = table.Column<string>(nullable: true),
                    ShowInCollection = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.FolderId);
                });

            migrationBuilder.CreateTable(
                name: "QueuedTracks",
                columns: table => new
                {
                    QueuedTrackId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: true),
                    SafePath = table.Column<string>(nullable: true),
                    IsPlaying = table.Column<bool>(nullable: false),
                    ProgressSeconds = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedTracks", x => x.QueuedTrackId);
                });

            migrationBuilder.CreateTable(
                name: "RemovedTracks",
                columns: table => new
                {
                    TrackId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: true),
                    SafePath = table.Column<string>(nullable: true),
                    DateRemoved = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemovedTracks", x => x.TrackId);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    TrackId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Artists = table.Column<string>(nullable: true),
                    Genres = table.Column<string>(nullable: true),
                    AlbumTitle = table.Column<string>(nullable: true),
                    AlbumArtists = table.Column<string>(nullable: true),
                    AlbumKey = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    SafePath = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: true),
                    BitRate = table.Column<long>(nullable: true),
                    SampleRate = table.Column<long>(nullable: true),
                    TrackTitle = table.Column<string>(nullable: true),
                    TrackNumber = table.Column<long>(nullable: true),
                    TrackCount = table.Column<long>(nullable: true),
                    DiscNumber = table.Column<long>(nullable: true),
                    DiscCount = table.Column<long>(nullable: true),
                    Duration = table.Column<long>(nullable: true),
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
                    table.PrimaryKey("PK_Tracks", x => x.TrackId);
                });

            migrationBuilder.CreateTable(
                name: "FolderTracks",
                columns: table => new
                {
                    FolderTrackId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderId = table.Column<long>(nullable: true),
                    TrackId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderTracks", x => x.FolderTrackId);
                    table.ForeignKey(
                        name: "FK_FolderTracks_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "FolderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FolderTracks_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderTracks_FolderId",
                table: "FolderTracks",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderTracks_TrackId",
                table: "FolderTracks",
                column: "TrackId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumArtworks");

            migrationBuilder.DropTable(
                name: "FolderTracks");

            migrationBuilder.DropTable(
                name: "QueuedTracks");

            migrationBuilder.DropTable(
                name: "RemovedTracks");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Tracks");
        }
    }
}
