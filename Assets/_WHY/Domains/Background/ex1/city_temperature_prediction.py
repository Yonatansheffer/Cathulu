import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from polynomial_fitting import PolynomialFitting

def load_data(filename: str) -> pd.DataFrame:
    """
    Load city daily temperature dataset and preprocess data.
    Parameters
    ----------
    filename: str
        Path to house prices dataset

    Returns
    -------
    Design matrix and response vector (Temp)
    """
    df = pd.read_csv(filename, parse_dates=["Date"])
    df.dropna(inplace=True)
    df = df[(df["Temp"] > -50) & (df["Temp"] < 60)]
    df["DayOfYear"] = df["Date"].dt.dayofyear
    return df


def train_test_split_custom(X: np.ndarray, y: np.ndarray, test_size: float = 0.25, random_seed: int = 42) -> tuple:
    """
    Custom train-test split function without using scikit-learn.

    Parameters
    ----------
    X : ndarray
        Feature array
    y : ndarray
        Target array
    test_size : float
        Proportion of dataset to include in the test split
    random_seed : int
        Seed for reproducibility

    Returns
    -------
    X_train, X_test, y_train, y_test : tuple of ndarrays
    """
    np.random.seed(random_seed)
    n_samples = len(X)
    n_test = int(n_samples * test_size)
    n_train = n_samples - n_test
    indices = np.arange(n_samples)
    np.random.shuffle(indices)

    train_indices = indices[:n_train]
    test_indices = indices[n_train:]

    X_train = X[train_indices]
    X_test = X[test_indices]
    y_train = y[train_indices]
    y_test = y[test_indices]

    return X_train, X_test, y_train, y_test

if __name__ == '__main__':
    # Question 2 - Load and preprocessing of city temperature dataset
    df = load_data("city_temperature.csv")

    # Question 3 - Exploring data for specific country
    israel_df = df[df["Country"] == "Israel"].copy()
    israel_df["Year"] = israel_df["Date"].dt.year

    years = israel_df["Year"].unique()
    colors = plt.cm.tab10(range(len(years)))
    plt.figure(figsize=(10, 6))
    for i, year in enumerate(sorted(years)):
        year_data = israel_df[israel_df["Year"] == year]
        plt.scatter(year_data["DayOfYear"], year_data["Temp"], color=colors[i], label=str(year), s=10)
    plt.title("Daily Average Temperature in Israel by Day of Year")
    plt.xlabel("Day of Year")
    plt.ylabel("Temperature (°C)")
    plt.legend(title="Year", bbox_to_anchor=(1.05, 1), loc="upper left")
    plt.tight_layout()
    #plt.savefig("israel_temp_scatter.png")

    monthly_std = israel_df.groupby("Month")["Temp"].std()

    plt.figure(figsize=(8, 5))
    plt.bar(monthly_std.index, monthly_std.values, color="skyblue", edgecolor="black")
    plt.title("Standard Deviation of Daily Temperatures in Israel by Month")
    plt.xlabel("Month")
    plt.ylabel("Temperature Std Dev (°C)")
    plt.xticks(range(1, 13))
    plt.tight_layout()
    #plt.savefig("israel_temp_std_by_month.png")

    # Question 4 - Exploring differences between countries
    grouped = df.groupby(['Country', 'Month'])["Temp"].agg(['mean', 'std']).reset_index()
    plt.figure(figsize=(10, 6))
    for country in grouped["Country"].unique():
        country_data = grouped[grouped["Country"] == country]
        plt.errorbar(
            country_data["Month"],
            country_data["mean"],
            yerr=country_data["std"],
            label=country,
            capsize=3
        )

    plt.title("Average Monthly Temperature by Country (with Std Dev Error Bars)")
    plt.xlabel("Month")
    plt.ylabel("Temperature (°C)")
    plt.xticks(range(1, 13))
    plt.legend()
    plt.tight_layout()
    #plt.savefig("avg_monthly_temp_by_country.png")

    # Question 5 - Fitting model for different values of `k`
    israel_df = df[df["Country"] == "Israel"].copy()
    X_israel = israel_df["DayOfYear"].values
    y_israel = israel_df["Temp"].values

    X_train, X_test, y_train, y_test = train_test_split_custom(X_israel, y_israel, test_size=0.25, random_seed=42)

    test_errors = []

    for k in range(1, 11):
        model = PolynomialFitting(k)
        model.fit(X_train, y_train)
        loss = model.loss(X_test, y_test)
        loss_rounded = round(loss, 2)
        test_errors.append(loss_rounded)
        #print(f"k = {k}, Test Error = {loss_rounded}")

    plt.figure(figsize=(8, 5))
    plt.bar(range(1, 11), test_errors, color="salmon", edgecolor="black")
    plt.title("Test Error vs. Polynomial Degree (k) for Israel")
    plt.xlabel("Polynomial Degree (k)")
    plt.ylabel("Mean Squared Error (MSE)")
    plt.xticks(range(1, 11))
    plt.tight_layout()
    #plt.savefig("israel_test_error_vs_k.png")

    # Question 6 - Evaluating fitted model on different countries
    X_israel = israel_df["DayOfYear"].values
    y_israel = israel_df["Temp"].values

    model = PolynomialFitting(k=2)
    model.fit(X_israel, y_israel)

    other_countries = df["Country"].unique()
    other_countries = other_countries[other_countries != "Israel"]

    errors = {}
    for country in other_countries:
        country_df = df[df["Country"] == country]
        X_country = country_df["DayOfYear"].values
        y_country = country_df["Temp"].values

        if len(X_country) > 0:
            country_loss = model.loss(X_country, y_country)
            errors[country] = round(country_loss, 2)

    sorted_errors = dict(sorted(errors.items(), key=lambda item: item[1]))
    plt.figure(figsize=(12, 6))
    plt.bar(sorted_errors.keys(), sorted_errors.values(), color="lightgreen", edgecolor="black")
    plt.xticks(rotation=45, ha="right")
    plt.ylabel("Mean Squared Error (MSE)")
    plt.title("Model Error (k=2) on Other Countries")
    plt.tight_layout()
    plt.savefig("israel")

