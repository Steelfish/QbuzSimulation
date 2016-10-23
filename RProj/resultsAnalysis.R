##########################################
plotNormalDistribution <- function(x, mean, sd, xlabel, subject) {
    # Plot the histogram with normal density curve.
    # Source: https://www.r-bloggers.com/exploratory-data-analysis-combining-histograms-and-density-plots-to-examine-the-distribution-of-the-ozone-pollution-data-from-new-york-in-r/
    png(paste('img/plot_normal-histogram_', subject, '.png', sep=''))
    hist(x, breaks=5, freq=FALSE, xlab=xlabel, ylab="Density", col="lightgray",
         main=paste("Histogram of", subject, "with Normal Density Curve\nmean=", round(mean, 2), "sd=", round(sd, 2)))
    curve(dnorm(x, mean=mean, sd=sd), col="red", add=TRUE)
    dev.off()
}


setting <- "55800-4-5-8"
results <- read.csv(paste("output/measurements_", setting, ".csv", sep=""), sep=";")

colnames(results) <- c("DelayPercentage","AverageDelay","MaximumDelay",
                       "PassengerDelayPercentage","AverageWaitingTime","MaximumWaitingTime",
                       "PassengersTransported","MaximumQueueLength","AverageQueueLength")

results$DelayPercentage = sub(",",".",results$DelayPercentage)
results$DelayPercentage  = as.numeric(results$DelayPercentage)
results$AverageDelay = sub(",",".",results$AverageDelay)
results$AverageDelay  = as.numeric(results$AverageDelay)
results$PassengerDelayPercentage = sub(",",".",results$PassengerDelayPercentage)
results$PassengerDelayPercentage = as.numeric(results$PassengerDelayPercentage)
results$AverageWaitingTime = sub(",",".",results$AverageWaitingTime)
results$AverageWaitingTime = as.numeric(results$AverageWaitingTime)


plotNormalDistribution(results$DelayPercentage, mean(results$DelayPercentage), sd(results$DelayPercentage),
                       "Delay percentage", "delayPercentage")

plotNormalDistribution(results$AverageDelay, mean(results$AverageDelay), sd(results$AverageDelay),
                       "Average delay", "averageDelay")

plotNormalDistribution(results$MaximumDelay, mean(results$MaximumDelay), sd(results$MaximumDelay),
                       "Maximum delay", "maximumDelay")

plotNormalDistribution(results$PassengerDelayPercentage, mean(results$PassengerDelayPercentage), sd(results$PassengerDelayPercentage),
                       "Passenger delay percentage", "passengerDelayPercentage")

plotNormalDistribution(results$AverageWaitingTime, mean(results$AverageWaitingTime), sd(results$AverageWaitingTime),
                       "Average waiting time", "averageWaitingTime")

plotNormalDistribution(results$MaximumWaitingTime, mean(results$MaximumWaitingTime), sd(results$MaximumWaitingTime),
                       "Maximum waiting time", "maximumWaitingTime")

plotNormalDistribution(results$PassengersTransported, mean(results$PassengersTransported), sd(results$PassengersTransported),
                       "Passengers transported", "passengersTransported")

plotNormalDistribution(results$MaximumQueueLength, mean(results$MaximumQueueLength), sd(results$MaximumQueueLength),
                       "Maximum queue length", "maximumQueueLength")

plotNormalDistribution(results$AverageQueueLength, mean(results$AverageQueueLength), sd(results$AverageQueueLength),
                       "Average queue length", "averageQueueLength")