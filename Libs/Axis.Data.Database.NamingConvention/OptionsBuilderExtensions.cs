using Axis.Data.Database.NamingConvention.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Globalization;

namespace Axis.Data.Database.NamingConvention;

public static class OptionsBuilderExtensions {

  public static DbContextOptionsBuilder UseSnakeCaseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!) {
    Check.NotNull(optionsBuilder, nameof(optionsBuilder));
    var extension =
      (optionsBuilder.Options.FindExtension<NamingConventionsOptionsExtension>() ?? new NamingConventionsOptionsExtension())
        .WithSnakeCaseNamingConvention(culture);
    ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
    return optionsBuilder;
  }

  public static DbContextOptionsBuilder<TContext> UseSnakeCaseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!) where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseSnakeCaseNamingConvention(culture);

  public static DbContextOptionsBuilder UseLowerCaseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!) {
    Check.NotNull(optionsBuilder, nameof(optionsBuilder));
    var extension =
      (optionsBuilder.Options.FindExtension<NamingConventionsOptionsExtension>() ?? new NamingConventionsOptionsExtension())
        .WithLowerCaseNamingConvention(culture);
    ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
    return optionsBuilder;
  }

  public static DbContextOptionsBuilder<TContext> UseLowerCaseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!) where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseLowerCaseNamingConvention(culture);

  public static DbContextOptionsBuilder UseUpperCaseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!) {
    Check.NotNull(optionsBuilder, nameof(optionsBuilder));
    var extension =
      (optionsBuilder.Options.FindExtension<NamingConventionsOptionsExtension>() ?? new NamingConventionsOptionsExtension())
        .WithUpperCaseNamingConvention(culture);
    ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
    return optionsBuilder;
  }

  public static DbContextOptionsBuilder<TContext> UseUpperCaseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!) where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseUpperCaseNamingConvention(culture);

  public static DbContextOptionsBuilder UseUpperSnakeCaseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!) {
    Check.NotNull(optionsBuilder, nameof(optionsBuilder));
    var extension =
      (optionsBuilder.Options.FindExtension<NamingConventionsOptionsExtension>() ?? new NamingConventionsOptionsExtension())
        .WithUpperSnakeCaseNamingConvention(culture);
    ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
    return optionsBuilder;
  }

  public static DbContextOptionsBuilder<TContext> UseUpperSnakeCaseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!) where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseUpperSnakeCaseNamingConvention(culture);

  public static DbContextOptionsBuilder UseCamelCaseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!) {
    Check.NotNull(optionsBuilder, nameof(optionsBuilder));
    var extension =
      (optionsBuilder.Options.FindExtension<NamingConventionsOptionsExtension>() ?? new NamingConventionsOptionsExtension())
        .WithCamelCaseNamingConvention(culture);
    ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
    return optionsBuilder;
  }

  public static DbContextOptionsBuilder<TContext> UseCamelCaseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!)
    where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseCamelCaseNamingConvention(culture);

  public static DbContextOptionsBuilder UseNamingConvention(
    [NotNull] this DbContextOptionsBuilder optionsBuilder, CultureInfo culture = default!, string name = default!) {
    return name switch {
      "SnakeCase" => optionsBuilder.UseSnakeCaseNamingConvention(culture),
      "LowerCase" => optionsBuilder.UseLowerCaseNamingConvention(culture),
      "UpperCase" => optionsBuilder.UseUpperCaseNamingConvention(culture),
      "UpperSnakeCase" => optionsBuilder.UseUpperSnakeCaseNamingConvention(culture),
      "CamelCase" => optionsBuilder.UseCamelCaseNamingConvention(culture),
      _ => optionsBuilder.UseCamelCaseNamingConvention(culture),
    };
  }

  public static DbContextOptionsBuilder<TContext> UseNamingConvention<TContext>(
    [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder, CultureInfo culture = default!, string name = default!)
    where TContext : DbContext
    => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseNamingConvention(culture, name);
}