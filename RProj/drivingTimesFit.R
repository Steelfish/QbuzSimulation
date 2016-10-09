##########################################
# Tram properties Nieuwegeinlijn fitting
##########################################
# The driving times for the tram on the Nieuwegein tram line.
nieuwegein <- read.csv("runtimes.csv",
                       header= TRUE, sep=";")

# The average driving time per stop.
averageNieuwegein <- colMeans(nieuwegein)


analyzeDrivingTimes <- function(drivingTimes)
{
    for(stopName in names(drivingTimes))
    {
        analyzeDrivingTimesPerStop(drivingTimes=drivingTimes[,stopName], stopName=stopName)
    }
}


analyzeDrivingTimesPerStop <- function(drivingTimes, stopName)
{
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
    gf <- goodfit(drivingTimes, type="poisson", method="MinChisq")
    summary(gf)
    
    
    # Plot the distributions.    
    plotNormalDistribution(drivingTimes, mean=normalFit$estimate[1], sd=normalFit$estimate[2],
                           xlabel="Driving time (seconds)", ylabel="Density", subject=paste("driving times (", stopName, ")",)
                           stopName)
    
    plotGammaDistribution(drivingTimes, shape=gammaFit$estimate[2], scale=gammaFit$estimate[1],
                          xlabel="Driving time (seconds)", ylabel="Frequency", subject=paste("driving times (", stopName, ")")
                          stopName)
}



plotNormalDistribution <- function(x, mean, sd, xlabel, ylabel, subject, stopName) {
    # Plot the histogram with normal density curve.
    # Source: https://www.r-bloggers.com/exploratory-data-analysis-combining-histograms-and-density-plots-to-examine-the-distribution-of-the-ozone-pollution-data-from-new-york-in-r/
    png(paste('img/plot_normal-histogram_', stopName, '.png', sep=''))
    hist(x, breaks=30, freq=FALSE, xlab=xlabel, ylab=ylabel, col="lightgray",
         main=paste("Histogram of", subject, "with Normal Density Curve"))
    curve(dnorm(x, mean=mean, sd=sd), col="red", add=TRUE)
    dev.off()
}

plotGammaDistribution <- function(x, shape, scale, xlabel, ylabel, subject, stopName) {
    # Plot the histogram with gamma density curve.
    png(paste('img/plot_gamma-histogram_', stopName, '.png', sep=''))
    hist(x, breaks=30, freq=FALSE, xlab=xlabel, ylab=ylabel, col="lightgray",
         main=paste("Histogram of", subject, "with Gamma Density Curve"))
    curve(dgamma(x, shape=shape, scale=scale), col="red", add=TRUE)
    dev.off()
}


analyzeDrivingTimes(nieuwegein)
