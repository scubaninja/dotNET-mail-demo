using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

// Proces tworzenia broadcastu jest następujący:
// Najpierw tworzone są dane początkowe (nazwa, slug)
// Następnie email i segment do wysłania, który jest określany przez tag (lub nie)
// Jeśli początkowy przypadek użycia korzysta z dokumentu markdown, to powinien zawierać wszystko, czego potrzebujemy
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  /// <summary>
  /// Identyfikator broadcastu.
  /// </summary>
  public int? ID { get; set; }

  /// <summary>
  /// Identyfikator emaila.
  /// </summary>
  public int? EmailId { get; set; }

  /// <summary>
  /// Status broadcastu, domyślnie ustawiony na "pending".
  /// </summary>
  public string Status { get; set; } = "pending";

  /// <summary>
  /// Nazwa broadcastu.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Slug broadcastu.
  /// </summary>
  public string? Slug { get; set; }

  /// <summary>
  /// Adres email do odpowiedzi.
  /// </summary>
  public string? ReplyTo { get; set; }

  /// <summary>
  /// Tag określający segment do wysłania, domyślnie ustawiony na "*".
  /// </summary>
  public string SendToTag { get; set; } = "*";

  /// <summary>
  /// Data i czas utworzenia broadcastu, domyślnie ustawiony na bieżący czas UTC.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  /// <summary>
  /// Prywatny konstruktor, który uniemożliwia bezpośrednie tworzenie instancji klasy Broadcast z zewnątrz.
  /// </summary>
  private Broadcast()
}