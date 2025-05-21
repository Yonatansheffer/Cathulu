import os

from linear_regression import LinearRegression
import matplotlib
matplotlib.use('TkAgg')
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np


def preprocess_train(X: pd.DataFrame, y: pd.Series):
    data = X.copy()
    data["price"] = y

    data.dropna(inplace=True)
    data.drop_duplicates(inplace=True)
    data = data[data["sqft_lot15"] >= 0]
    data = data[data["yr_renovated"] >= data["yr_built"]]
    data = data[data["bedrooms"] > 0]
    if "id" in data.columns:
        data.drop("id", axis=1, inplace=True)
    data["house_age"] = 2015 - data["yr_built"]
    data["was_renovated"] = data["yr_renovated"] > 0
    data["total_rooms"] = data["bedrooms"] + data["bathrooms"]
    data["living_to_lot_ratio"] = data["sqft_living"] / data["sqft_lot"]
    y_cleaned = data["price"]
    X_cleaned = data.drop("price", axis=1)
    X_cleaned = X_cleaned.select_dtypes(include=["number", "bool"])
    for col in X_cleaned.select_dtypes(include=["bool"]).columns:
        X_cleaned[col] = X_cleaned[col].astype(int)

    X_cleaned = X_cleaned.fillna(X_cleaned.mean()).astype(float)
    y_cleaned = y_cleaned.astype(float)
    return X_cleaned, y_cleaned

def preprocess_test(X: pd.DataFrame):
    """
    Preprocess the test data. This function adds columns derived from the training data features.
    It does not remove rows, only adds/removes columns or modifies cells in the test set.

    Parameters
    ----------
    X: pd.DataFrame
        The loaded test data (features)

    Returns
    -------
    X_cleaned: pd.DataFrame
        A preprocessed version of the test data with the same structure as the training data
    """
    if "id" in X.columns:
        X = X.drop("id", axis=1)
    X["house_age"] = 2015 - X["yr_built"]
    X["was_renovated"] = X["yr_renovated"] > 0
    X["total_rooms"] = X["bedrooms"] + X["bathrooms"]
    X["living_to_lot_ratio"] = X["sqft_living"] / X["sqft_lot"]
    X_cleaned = X.select_dtypes(include=["number", "bool"])
    for col in X_cleaned.select_dtypes(include=["bool"]).columns:
        X_cleaned[col] = X_cleaned[col].astype(int)
    X_cleaned = X_cleaned.fillna(X_cleaned.mean()).astype(float)
    return X_cleaned


def feature_evaluation(X: pd.DataFrame, y: pd.Series, output_path: str = "."):
    for feature in X.columns:
        x = X[feature]

        std_x = np.std(x)
        std_y = np.std(y)
        if std_x == 0 or std_y == 0 or np.isnan(std_x) or np.isnan(std_y):
            continue

        covariance = np.cov(x, y)[0, 1]
        pearson_corr = covariance / (std_x * std_y)
        plt.figure()
        plt.scatter(x, y, alpha=0.5)
        plt.xlabel(feature)
        plt.ylabel("Price")
        plt.title(f"{feature} vs Price\nPearson Correlation = {pearson_corr:.3f}")

        filename = f"{output_path}/{feature}_pearson_correlation.png"
        plt.savefig(filename)
        plt.close()


def split_train_test(X, y, test_size=0.25, random_state=42):
    np.random.seed(random_state)
    indices = np.arange(len(X))
    np.random.shuffle(indices)

    test_count = int(len(X) * test_size)
    test_indices = indices[:test_count]
    train_indices = indices[test_count:]

    X_train = X.iloc[train_indices].reset_index(drop=True)
    y_train = y.iloc[train_indices].reset_index(drop=True)
    X_test = X.iloc[test_indices].reset_index(drop=True)
    y_test = y.iloc[test_indices].reset_index(drop=True)

    return X_train, X_test, y_train, y_test


def evaluate_loss_over_percentages(X_train, y_train, X_test, y_test):
    percentages = list(range(10, 101))
    mean_losses = []
    std_losses = []

    # Convert to NumPy once, ensuring float type
    X_train_np = X_train.to_numpy(dtype=float)
    y_train_np = y_train.to_numpy(dtype=float)
    X_test_np = X_test.to_numpy(dtype=float)
    y_test_np = y_test.to_numpy(dtype=float)

    for p in percentages:
        losses = []
        n_samples = int(p / 100 * len(X_train_np))

        for i in range(10):
            # Sample indices
            sample_indices = np.random.choice(len(X_train_np), size=n_samples, replace=False)
            X_sample_np = X_train_np[sample_indices]
            y_sample_np = y_train_np[sample_indices]

            # Train model
            model = LinearRegression(include_intercept=True)
            model.fit(X_sample_np, y_sample_np)

            # Evaluate loss
            loss = model.loss(X_test_np, y_test_np)
            losses.append(loss)

        mean_losses.append(np.mean(losses))
        std_losses.append(np.std(losses))

    # Plotting
    plt.figure(figsize=(10, 6))
    plt.plot(percentages, mean_losses, label="Mean Test Loss", color="blue")
    plt.fill_between(percentages,
                     np.array(mean_losses) - 2 * np.array(std_losses),
                     np.array(mean_losses) + 2 * np.array(std_losses),
                     color="blue", alpha=0.2, label="± 2 std confidence interval")
    plt.xlabel("Percentage of Training Data Used")
    plt.ylabel("Mean Squared Error on Test Set")
    plt.title("Test Loss vs. Training Data Size")
    plt.yscale("log")  # Optional: log scale for better visualization
    plt.legend()
    plt.grid(True)
    plt.tight_layout()
    plt.savefig("test_loss_vs_training_percentage.png")
    plt.show()

if __name__ == '__main__':
    df = pd.read_csv("house_prices.csv")
    X, y = df.drop("price", axis=1), df["price"]

    # Question 2 - split train test
    X_train, X_test, y_train, y_test = split_train_test(X, y, test_size=0.25, random_state=42)

    # Question 3 - preprocessing of housing prices train dataset
    X_train, y_train = preprocess_train(X_train, y_train)

    # Question 4 - Feature evaluation of train dataset with respect to response
    os.makedirs("plots", exist_ok=True)  # Create folder if it doesn't exist
    feature_evaluation(X_train, y_train, output_path="plots")

    # Question 5 - preprocess the test data
    X_test = preprocess_test(X_test)

    # Question 6 - Fit model over increasing percentages of the overall training data
    evaluate_loss_over_percentages(X_train, y_train, X_test, y_test)
