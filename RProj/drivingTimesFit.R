##########################################
# Tram properties Nieuwegeinlijn fitting
##########################################
# The driving times for the tram on the Nieuwegein tram line.
nieuwegein <- read.csv("runtimes.csv",
                       header= TRUE, sep=";")

# The average driving time per stop.
averageNieuwegein <- colMeans(nieuwegein)

# The driving times to Graadt van Roggenweg.
drivingTimes <- nieuwegein[,1]

# Fit distributions
# https://www.r-bloggers.com/goodness-of-fit-test-in-r/
#install.packages("MASS")
library(MASS) ## loading package MASS
normalFit <- fitdistr(drivingTimes, "normal") ## fitting gaussian pdf parameters
gammaFit <- fitdistr(drivingTimes, "gamma") ## fitting gamma pdf parameters
weibullFit <- fitdistr(drivingTimes, "weibull") ## fitting Weibull pdf parameters


# Kolmogorov-Smirnov goodness test for the fitted distributions.
# Note that as the driving times are discrete, multiple occurrences of a value are possible.
# This is not allowed for the continuous test, thus we add small amounts of noise to the driving times.
ks.test(jitter(drivingTimes), "pnorm", mean=normalFit$estimate[1], sd=normalFit$estimate[2])
ks.test(jitter(drivingTimes), "pgamma", shape=gammaFit$estimate[2], rate=gammaFit$estimate[1])
ks.test(jitter(drivingTimes),"pweibull", shape=weibullFit$estimate[2], scale=weibullFit$estimate[1])

shapiro.test(drivingTimes)

# Chi-squared test
library(vcd)## loading vcd package
gf <- goodfit(x.poi, type="gamma", method="MinChisq")
summary(gf)


# Plot the distributions.
plot(x, dgamma(x, shape=gammaFit$estimate[2], scale=gammaFit$estimate[1]),
     col='blue',
     main="Gamma", type='l')

plot(x, dnorm(x, mean=normalFit$estimate[1], sd=normalFit$estimate[2]),
     col='blue',
     main="Normal distribution", type='l')

plot(x, dweibull(x, shape=weibullFit$estimate[2], scale=weibullFit$estimate[1]),
     col='blue',
     main="Weibull distribution", type='l')


plotNormalDistribution(drivingTimes, mean=normalFit$estimate[1], sd=normalFit$estimate[2],
                       xlabel="Driving time (seconds)", ylabel="Frequency", subject="driving times")

plotGammaDistribution(drivingTimes, shape=gammaFit$estimate[2], scale=gammaFit$estimate[1],
                      xlabel="Driving time (seconds)", ylabel="Frequency", subject="driving times")


plotNormalDistribution <- function(x, mean, sd, xlabel, ylabel, subject) {
    # Source: https://www.r-bloggers.com/exploratory-data-analysis-combining-histograms-and-density-plots-to-examine-the-distribution-of-the-ozone-pollution-data-from-new-york-in-r/
# The pdf curve and histogram plotted together.
#     h <- hist(drivingTimes, breaks=15)
#     xhist <- c(min(h$breaks), h$breaks)
#     yhist <- c(0, h$density, 0)
#     xfit <- seq(min(drivingTimes), max(drivingTimes), length=40)
#     yfit <- dnorm(xfit, mean=normalFit$estimate[1], sd=normalFit$estimate[2])
#     plot(xhist, yhist, type="s", ylim=c(0,max(yhist, yfit)), main="Normal pdf and
#          histogram")
#     lines(xfit, yfit, col="red")
    png('img/plot_normal-histogram.png')
    x.histogram = hist(x)
    x.ylim.normal = range(0, x.histogram$density, dnorm(x, mean, sd))
    hist(x, breaks=15, ylim=c(0, max(x.histogram$count)), xlab=xlabel, ylab=ylabel,
         main=paste("Histogram of", subject, "with Normal Density Curve"))
    curve(dnorm(x, mean, sd), col="red", add=TRUE)
    dev.off()
}

plotGammaDistribution <- function(x, shape, scale, xlabel, ylabel, subject) {
    # histogram with gamma density curve
    png('img/plot_gamma-histogram.png')
    hist(x, breaks=15, ylim=c(0, max(x)),
         xlab=xlabel, ylab=ylabel, main=paste("Histogram of", subject, "with Gamma Density Curve"))
    curve(dgamma(x, shape, scale), col="red", add=TRUE)
    dev.off()
}

